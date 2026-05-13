using Authorization.Domain.Entities;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Authorization.Domain.ValueObjects.Login
{
    public sealed class Email
    {
        public string Value { get; }

        private static readonly Regex EmailRegex =
            new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string rawEmail)
        {
            if (string.IsNullOrWhiteSpace(rawEmail))
                throw new ValidationDomainException(
                    DomainErrorCodes.LoginError.Empty,
                    $"{nameof(rawEmail)} validation failed: value is null. Entity {nameof(Email)}!"
                );

            var trimmed = rawEmail.Trim();

            if (!EmailRegex.IsMatch(trimmed))
                throw new ValidationDomainException(
                    DomainErrorCodes.LoginError.Invalid, 
                    $"Invalid email format. Entity {nameof(Email)}!"
                );

            var normalized = trimmed.ToLowerInvariant();

            return new Email(normalized);
        }

        public static Email Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference, 
                    $"Email cannot be empty on restore. Entity {nameof(Email)}!"
                );

            return new Email(value.ToLowerInvariant());
        }

        public static bool IsValidFormat(string rawEmail)
        {
            if (string.IsNullOrWhiteSpace(rawEmail)) 
                return false;

            return EmailRegex.IsMatch(rawEmail.Trim());
        }

        public static bool TryCreate(string rawEmail, out Email? email)
        {
            if (!IsValidFormat(rawEmail))
            {
                email = null;
                return false;
            }

            email = new Email(rawEmail.Trim().ToLowerInvariant());

            return true;
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            if (obj is not Email other) return false;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Email? left, Email? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Email? left, Email? right) => !(left == right);
    }
}
