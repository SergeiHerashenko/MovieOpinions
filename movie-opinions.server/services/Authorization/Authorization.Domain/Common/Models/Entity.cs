using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.Common.Models
{
    public abstract class Entity<TId> : IEquatable<Entity<TId>>, IHasDomainEvents
        where TId : notnull
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        private readonly List<IAggregateChange> _aggregateChanges = new();

        public TId Id { get; protected set; }

        public DateTimeOffset CreatedAt { get; protected set; }

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public IReadOnlyList<IAggregateChange> AggregateChanges => _aggregateChanges.AsReadOnly();

        protected Entity(TId id, DateTimeOffset? createdAt = null)
        {
            Id = id;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        }

        // Порівнює сутності за їх ідентифікаторами.
        public override bool Equals(object? obj)
            => obj is Entity<TId> entity && Id.Equals(entity.Id);

        // Типізована реалізація порівняння.
        public bool Equals(Entity<TId>? other)
            => Equals((object?)other);

        public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
            => Equals(left, right);

        public static bool operator !=(Entity<TId>? left,Entity<TId>? right)
            => !Equals(left, right);

        // Хеш-код сутності базується виключно на її унікальному ідентифікаторі.
        public override int GetHashCode()
            => Id.GetHashCode();

        // Додає нову подію до списку
        public void AddDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void AddAggregateChange(IAggregateChange aggregateChang)
            => _aggregateChanges.Add(aggregateChang);

        // Очищає список подій після їх успішної обробки
        public void ClearDomainEvents()
            => _domainEvents.Clear();

        public void ClearAggregateChanges()
            => _aggregateChanges.Clear();
    }
}