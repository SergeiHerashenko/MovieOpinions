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

        public bool Equals(ValueObject? other)
            => Equals((object?)other);

        public static bool operator ==(
            ValueObject? left, 
            ValueObject? right)
            => Equals(left, right);

        public static bool operator !=(
            ValueObject? left, 
            ValueObject? right)
            => !Equals(left, right);

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