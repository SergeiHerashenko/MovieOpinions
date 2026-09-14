namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий клас для строго типізованих ідентифікаторів.
    /// Гарантує наявність скалярного значення та реалізує рівність за ним.
    ///
    /// (Base class for strongly typed identifiers.
    /// Guarantees a scalar value and implements equality based on it.)
    /// </summary>
    /// <typeparam name="TId">Тип скалярного значення ідентифікатора.</typeparam>
    public abstract class StronglyTypedId<TId> : ValueObject
        where TId : notnull
    {
        public abstract TId Value { get; }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
