namespace Authorization.Domain.Common.Models
{
    /// Базовий абстрактний клас для строго типізованих
    /// ідентифікаторів коренів агрегатів.
    ///
    /// (Base abstract class for strongly typed
    /// aggregate-root identifiers.)
    public abstract class AggregateRootId<TId> : StronglyTypedId<TId>
        where TId : notnull
    { } 
}
