using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.ValueObjects.Login
{
    public sealed class Phone
    {
        public string Value { get; }

        private Phone(string value)
        {
            Value = value;
        }

        public static Phone Create(string rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone))
                throw new ValidationDomainException(
                    ErrorCodes.LoginError.Empty,
                    $"{nameof(rawPhone)} validation failed: value is null. Entity {nameof(Phone)}!"
                );

            var cleaned = CleanPhoneNumber(rawPhone);

            if (!IsValidE164(cleaned))
                throw new ValidationDomainException(
                    ErrorCodes.LoginError.InvalidPhone,
                    $"Invalid phone format. Expected E.164 format. Entity {nameof(Phone)}!"
                );

            return new Phone(cleaned);
        }

        public static Phone Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !IsValidE164(value))
                throw new DataInconsistencyDomainException(
                    ErrorCodes.RestoreError.NullReference,
                    $"Invalid phone format on restore. Entity {nameof(Phone)}!"
                );

            return new Phone(value);
        }

        public static bool IsValidFormat(string rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone)) return false;

            var cleaned = CleanPhoneNumber(rawPhone);

            return IsValidE164(cleaned);
        }

        public static bool TryCreate(string rawPhone, out Phone? phone)
        {
            if (!IsValidFormat(rawPhone))
            {
                phone = null;
                return false;
            }

            phone = new Phone(CleanPhoneNumber(rawPhone));

            return true;
        }

        private static string CleanPhoneNumber(string phone)
        {
            var onlyDigitsAndPlus = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

            if (!onlyDigitsAndPlus.StartsWith("+"))
                onlyDigitsAndPlus = "+" + onlyDigitsAndPlus.TrimStart('0', '+');

            return onlyDigitsAndPlus;
        }

        private static bool IsValidE164(string phone)
        {
            return phone.StartsWith("+") &&
               phone.Length >= 8 &&
               phone.Length <= 16 &&
               phone.Skip(1).All(char.IsDigit);
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            if (obj is not Phone other) return false;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Phone? left, Phone? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Phone? left, Phone? right) => !(left == right);
    }
}
