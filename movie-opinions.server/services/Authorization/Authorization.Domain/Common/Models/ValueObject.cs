namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий клас для об'єктів-значень доменної моделі.
    /// Два Value Object вважаються рівними, якщо вони мають однаковий
    /// конкретний тип і однакову впорядковану послідовність компонентів рівності.
    ///
    /// (Base class for domain value objects. Two value objects are considered
    /// equal when they have the same concrete type and the same ordered sequence
    /// of equality components.)
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Повертає впорядковану послідовність незмінних компонентів,
        /// які повністю визначають рівність Value Object.
        /// Порядок компонентів повинен бути стабільним.
        ///
        /// (Returns an ordered sequence of immutable components that fully
        /// define value-object equality. Component order must remain stable.)
        /// </summary>
        public abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var valueObject = (ValueObject)obj;

            return GetEqualityComponents()
                .SequenceEqual(valueObject.GetEqualityComponents());
        }

        /// <inheritdoc/>
        public bool Equals(ValueObject? other)
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
            ValueObject? left,
            ValueObject? right)
            => Equals(left, right);

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
            ValueObject? left,
            ValueObject? right)
            => !Equals(left, right);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var component in GetEqualityComponents())
            {
                hashCode.Add(component);
            }

            return hashCode.ToHashCode();
        }
    }
}
