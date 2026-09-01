namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий клас для коренів агрегатів.
    /// Гарантує використання строго типізованого ідентифікатора.
    ///
    /// (Base class for aggregate roots.
    /// Ensures that each aggregate root uses a strongly typed identifier.)
    /// </summary>
    /// <typeparam name="TId">Тип строго типізованого ідентифікатора.</typeparam>
    /// <typeparam name="TIdType">Скалярний тип значення ідентифікатора.</typeparam>
    public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
        where TIdType : notnull
        where TId : AggregateRootId<TIdType>
    {
        /// <summary>
        /// Ініціалізує новий екземпляр кореня агрегату.
        /// 
        /// (Initializes a new instance of the aggregate root.)
        /// </summary>
        /// <param name="id">Строго типізований ідентифікатор агрегату.</param>
        /// <param name="createdAt">Дата створення агрегату. Якщо null, використовується поточний UTC-час.</param>
        protected AggregateRoot(
            TId id, 
            DateTimeOffset? createdAt = null) 
            : base(id, createdAt) { }
    }
}
