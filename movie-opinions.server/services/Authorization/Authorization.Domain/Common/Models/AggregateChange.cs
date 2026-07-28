using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.Common.Models
{
    public class AggregateChange : IAggregateChange
    {
        public DateTimeOffset OccurredOn { get; }

        protected AggregateChange(DateTimeOffset occurredOn)
        {
            OccurredOn = occurredOn;
        }
    }
}
