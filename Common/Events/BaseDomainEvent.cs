using MediatR;

namespace Common.Events
{
    public abstract class BaseDomainEvent : INotification
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }

        protected BaseDomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
