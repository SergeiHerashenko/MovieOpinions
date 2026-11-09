namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий клас для коренів агрегатів.
    /// Гарантує використання строго типізованого ідентифікатора.
    ///
    /// (Base class for aggregate roots.
    /// Ensures that each aggregate root uses a strongly typed identifier.)
    /// </summary>
    /// <typeparam name="TId">
    /// Тип строго типізованого ідентифікатора.
    ///
    /// (The strongly typed identifier type.)
    /// </typeparam>
    /// <typeparam name="TIdType">
    /// Тип значення ідентифікатора.
    ///
    /// (The underlying identifier value type.)
    /// </typeparam>
    public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
        where TIdType : notnull
        where TId : AggregateRootId<TIdType>
    {
        /// <summary>
        /// Ініціалізує новий екземпляр кореня агрегату.
        ///
        /// (Initializes a new instance of the aggregate root.)
        /// </summary>
        /// <param name="id">
        /// Строго типізований ідентифікатор агрегату.
        ///
        /// (The strongly typed aggregate identifier.)
        /// </param>
        /// <param name="createdAt">
        /// Дата та час створення агрегату.
        /// Якщо null, значення визначається базовим класом Entity.
        ///
        /// (The aggregate creation date and time.
        /// If null, the value is determined by the Entity base class.)
        /// </param>
        protected AggregateRoot(
            TId id,
            DateTimeOffset? createdAt = null)
            : base(id, createdAt)
        { }
    }
}
