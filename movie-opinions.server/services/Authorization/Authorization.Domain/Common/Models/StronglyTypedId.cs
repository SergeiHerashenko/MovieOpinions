namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий клас для строго типізованих ідентифікаторів.
    /// Рівність визначається конкретним типом ідентифікатора
    /// та його значенням.
    ///
    /// (Base class for strongly typed identifiers.
    /// Equality is determined by the concrete identifier type
    /// and its value.)
    /// </summary>
    /// <typeparam name="TId">
    /// Тип значення ідентифікатора.
    /// 
    /// (The underlying identifier value type.)
    /// </typeparam>
    public abstract class StronglyTypedId<TId> : ValueObject
    {
        /// <summary>
        /// Значення ідентифікатора.
        /// 
        /// (The identifier value.)
        /// </summary>
        public abstract TId Value { get; }

        /// <summary>
        /// Повертає впорядковану послідовність незмінних компонентів,
        /// які визначають рівність Value Object одного конкретного типу.
        /// Порядок компонентів повинен бути стабільним.
        ///
        /// (Returns an ordered sequence of immutable components that determine
        /// equality between value objects of the same concrete type.
        /// Component order must remain stable.)
        /// </summary>
        /// <returns>
        /// Впорядкована послідовність компонентів рівності.
        /// 
        /// (An ordered sequence of equality components.)
        /// </returns>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
