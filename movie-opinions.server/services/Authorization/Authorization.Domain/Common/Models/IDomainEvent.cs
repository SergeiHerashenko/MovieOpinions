namespace Authorization.Domain.Common.Models
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredOn { get; }
    }
}
