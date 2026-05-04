namespace Authorization.Domain.DomainEvents
{
    public class DomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; }

        protected DomainEvent(DateTime occurredOn)
        {
            OccurredOn = occurredOn;
        }
    }
}
