using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий клас доменних сутностей.
    /// Ідентичність і рівність сутності визначаються її конкретним типом та ID.
    /// Також накопичує доменні події й зміни агрегату до моменту їх обробки.
    /// 
    /// (Base class for domain entities. 
    /// An entity's identity and equality are determined by its concrete type and ID.
    /// Also accumulates domain events and aggregate changes until they are processed.)
    /// </summary>
    /// <typeparam name="TId">
    /// Тип унікального ідентифікатора сутності.
    ///
    /// (The unique entity identifier type.)
    /// </typeparam>
    public abstract class Entity<TId> : IEquatable<Entity<TId>>, IHasDomainEvents, IHasAggregateChanges
        where TId : notnull
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        private readonly List<IAggregateChange> _aggregateChanges = new();

        #region Properties
        /// <summary>
        /// Унікальний ідентифікатор сутності.
        /// 
        /// (Unique entity identifier.)
        /// </summary>
        public TId Id { get; }

        /// <summary>
        /// Дата та час створення сутності.
        /// 
        /// (Entity creation date and time.)
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Накопичені доменні події, що очікують публікації або очищення.
        /// 
        /// (Accumulated domain events awaiting dispatch or clearing.)
        /// </summary>
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Накопичені зміни агрегату, що очікують застосування та очищення.
        /// 
        /// (Accumulated aggregate changes awaiting processing and clearing.)
        /// </summary>
        public IReadOnlyList<IAggregateChange> AggregateChanges => _aggregateChanges.AsReadOnly();
        #endregion

        /// <summary>
        /// Ініціалізує новий екземпляр сутності.
        /// 
        /// (Initializes a new instance of the entity.)
        /// </summary>
        /// <param name="id">
        /// Унікальний ідентифікатор сутності.
        ///
        /// (The unique entity identifier.)
        /// </param>
        /// <param name="createdAt">
        /// Дата створення (якщо null — встановлюється поточний UtcNow).
        ///
        /// (Creation date (if null, the current UtcNow is set).)
        /// </param>
        protected Entity(TId id, DateTimeOffset? createdAt = null)
        {
            Id = id;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var entity = (Entity<TId>)obj;

            return EqualityComparer<TId>.Default.Equals(Id, entity.Id);
        }

        /// <inheritdoc/>
        public bool Equals(Entity<TId>? other)
            => Equals((object?)other);

        /// <summary>
        /// Порівнює два об’єкти на рівність.
        /// 
        /// (Compares two objects for equality.)
        /// </summary>
        /// <param name="left">
        /// Лівий операнд.
        /// 
        /// (The left operand.)
        /// </param>
        /// <param name="right">
        /// Правий операнд.
        ///
        /// (The right operand.)
        /// </param>
        /// <returns>
        /// true, якщо об’єкти рівні або обидва дорівнюють null; інакше false.
        /// 
        /// (true if the objects are equal or both are null; otherwise, false.)
        /// </returns>
        public static bool operator ==(
            Entity<TId>? left,
            Entity<TId>? right)
            => object.Equals(left, right);

        /// <summary>
        /// Порівнює два об’єкти на нерівність.
        /// 
        /// (Compares two objects for inequality.)
        /// </summary>
        /// <param name="left">
        /// Лівий операнд.
        ///
        /// (The left operand.)
        /// </param>
        /// <param name="right">
        /// Правий операнд.
        ///
        /// (The right operand.)
        /// </param>
        /// <returns>
        /// true, якщо об’єкти нерівні; інакше false.
        /// 
        /// (true if the objects are unequal; otherwise, false.)
        /// </returns>
        public static bool operator !=(
            Entity<TId>? left,
            Entity<TId>? right)
            => !object.Equals(left, right);

        /// <inheritdoc/>
        public override int GetHashCode()
            => HashCode.Combine(GetType(), Id);

        /// <summary>
        /// Реєструє нову доменну подію.
        /// 
        /// (Registers a new domain event.)
        /// </summary>
        /// <param name="domainEvent">
        /// Екземпляр доменної події.
        ///
        /// (An instance of a domain event.)
        /// </param>
        protected void AddDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        /// <summary>
        /// Реєструє зміну агрегату.
        /// 
        /// (Registers an aggregate change.)
        /// </summary>
        /// <param name="aggregateChange">
        /// Екземпляр зміни агрегату.
        ///
        /// (Aggregate change instance)
        /// </param>
        protected void AddAggregateChange(IAggregateChange aggregateChange)
            => _aggregateChanges.Add(aggregateChange);

        /// <summary>
        /// Очищає список доменних подій після їх публікації.
        /// 
        /// (Clears the list of domain events after dispatch.)
        /// </summary>
        public void ClearDomainEvents()
            => _domainEvents.Clear();

        /// <summary>
        /// Очищає список змін агрегату.
        /// 
        /// (Clears the list of aggregate changes.)
        /// </summary>
        public void ClearAggregateChanges()
            => _aggregateChanges.Clear();
    }
}
