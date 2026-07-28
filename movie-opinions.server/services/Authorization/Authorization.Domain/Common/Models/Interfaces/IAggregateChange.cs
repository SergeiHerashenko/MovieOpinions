namespace Authorization.Domain.Common.Models.Interfaces
{
    public interface IAggregateChange
    {
        DateTimeOffset OccurredOn { get; }
    }
}
