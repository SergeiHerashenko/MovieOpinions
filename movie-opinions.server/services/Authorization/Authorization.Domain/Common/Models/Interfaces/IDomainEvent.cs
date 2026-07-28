namespace Authorization.Domain.Common.Models.Interfaces
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredOn { get; }
    }
}
