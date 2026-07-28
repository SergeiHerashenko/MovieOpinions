using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.Common.Models
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
