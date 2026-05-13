using Authorization.Domain.DomainEvents;

namespace Authorization.Domain.Common
{
    public class AggregateRoot : BaseEntity, IHasDomainEvents
    {
        protected AggregateRoot() : base() { }

        protected AggregateRoot(Guid id, DateTimeOffset createdAt)
            : base(id, createdAt) { }

        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void ClearDomainEvents() => _domainEvents.Clear();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
