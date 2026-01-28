using EventBus.Events;

namespace EventBus.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : IntegrationEvent;

        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
