namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий абстрактний клас для строго типізованих ідентифікаторів агрегатів.
    /// Використовує скалярне значення <see cref="Value"/> як єдиний компонент рівності.
    /// Правила валідності значення визначаються конкретним типом ідентифікатора.
    ///
    /// (Base abstract class for strongly typed aggregate identifiers.
    /// Uses the scalar <see cref="Value"/> as the sole equality component.
    /// Value validation rules are defined by each concrete identifier type.)
    /// </summary>
    public abstract class AggregateRootId<TId> : ValueObject
        where TId : notnull
    {
        /// <summary>
        /// Скалярне значення ідентифікатора.
        /// 
        /// (The scalar value of the identifier.)
        /// </summary>
        public abstract TId Value { get; }

        /// <summary>
        /// Повертає значення ідентифікатора як єдиний компонент для порівняння структурної рівності.
        /// 
        /// (Returns the identifier value as the single component for structural equality comparison.)
        /// </summary>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}