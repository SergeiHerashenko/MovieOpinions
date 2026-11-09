namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий абстрактний клас для строго типізованих
    /// ідентифікаторів коренів агрегатів.
    ///
    /// (Base abstract class for strongly typed
    /// aggregate root identifiers.)
    /// </summary>
    /// <typeparam name="TId">
    /// Тип значення ідентифікатора.
    ///
    /// (The underlying identifier value type.)
    /// </typeparam>
    public abstract class AggregateRootId<TId> : StronglyTypedId<TId>
        where TId : notnull
    { }
}
