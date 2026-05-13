namespace Authorization.Domain.DomainEvents
{
    public class DomainEvent : IDomainEvent
    {
        public DateTimeOffset OccurredOn { get; }

        protected DomainEvent(DateTimeOffset occurredOn)
        {
            OccurredOn = occurredOn;
        }
    }
}
