namespace Authorization.Domain.Common.Models
{
    public interface IHasDomainEvents
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }

        void ClearDomainEvents();
    }
}
