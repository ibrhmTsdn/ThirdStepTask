namespace EventBus.Events
{
    public abstract class IntegrationEvent
    {
        public Guid Id { get; }
        public DateTime CreatedDate { get; }

        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        protected IntegrationEvent(Guid id, DateTime createDate)
        {
            Id = id;
            CreatedDate = createDate;
        }
    }
}
