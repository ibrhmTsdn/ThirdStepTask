using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace EventBus.RabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retryCount;
        private IModel? _consumerChannel;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        public RabbitMQEventBus(
            IRabbitMQPersistentConnection persistentConnection,
            ILogger<RabbitMQEventBus> logger,
            IServiceProvider serviceProvider,
            int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider;
            _retryCount = retryCount;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }

        public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = CreateRetryPolicy();

            var eventName = @event.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

            using var channel = _persistentConnection.CreateModel();

            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

            channel.ExchangeDeclare(exchange: "microservices_event_bus", type: "direct");

            var message = JsonSerializer.Serialize(@event, @event.GetType());
            var body = Encoding.UTF8.GetBytes(message);

            await policy.ExecuteAsync(async () =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                channel.BasicPublish(
                    exchange: "microservices_event_bus",
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                await Task.CompletedTask;
            });
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (_handlers.ContainsKey(eventName))
            {
                _handlers[eventName].Remove(handlerType);

                if (_handlers[eventName].Count == 0)
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                }
            }
        }

        private void StartBasicConsume<T>() where T : IntegrationEvent
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _consumerChannel = _persistentConnection.CreateModel();

            _consumerChannel.ExchangeDeclare(exchange: "microservices_event_bus", type: "direct");

            var queueName = typeof(T).Name + "_queue";

            _consumerChannel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _consumerChannel.QueueBind(
                queue: queueName,
                exchange: "microservices_event_bus",
                routingKey: typeof(T).Name);

            var consumer = new EventingBasicConsumer(_consumerChannel);

            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    await ProcessEvent(eventName, message);
                    _consumerChannel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing message: {Message}", message);
                    _consumerChannel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _consumerChannel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (!_handlers.ContainsKey(eventName))
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var subscriptions = _handlers[eventName];

            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider.GetService(subscription);
                if (handler == null) continue;

                var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                if (eventType == null) continue;

                var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                if (integrationEvent == null) continue;

                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                await (Task)concreteType.GetMethod("Handle")!.Invoke(handler, new[] { integrationEvent })!;
            }
        }

        private Polly.Retry.AsyncRetryPolicy CreateRetryPolicy()
        {
            return Polly.Policy.Handle<Exception>()
                .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex, "Could not publish event after {Timeout}s ({ExceptionMessage})",
                            $"{time.TotalSeconds:n1}", ex.Message);
                    });
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();
        }
    }
}
