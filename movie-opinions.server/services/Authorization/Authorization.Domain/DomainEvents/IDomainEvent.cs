namespace Authorization.Domain.DomainEvents
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredOn { get; }
    }
}
