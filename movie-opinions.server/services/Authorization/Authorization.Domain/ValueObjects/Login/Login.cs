using Authorization.Domain.Enums;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using System.Numerics;

namespace Authorization.Domain.ValueObjects.Login
{
    public sealed class Login
    {
        public string Value { get; }

        public LoginType Type { get; }

        private Login(string value, LoginType type)
        {
            Value = value;
            Type = type;
        }

        public static Login FromEmail(Email email) => new(email.Value, LoginType.Email);

        public static Login FromPhone(Phone phone) => new(phone.Value, LoginType.Phone);

        public static Login Create(string rawLogin)
        {
            if (string.IsNullOrWhiteSpace(rawLogin))
                throw new ValidationDomainException(
                    ErrorCodes.LoginError.Empty,
                    $"{nameof(rawLogin)} validation failed: value is null. Entity {nameof(Login)}!"
                );

            if (Email.TryCreate(rawLogin, out var email))
                return FromEmail(email!);

            if (Phone.TryCreate(rawLogin, out var phone))
                return FromPhone(phone!);

            throw new ValidationDomainException(
                ErrorCodes.LoginError.InvalidEmail, 
                $"Invalid login format. Expected Email. Entity {nameof(Login)}!"
            );
        }

        public static Login Restore(string value, LoginType type)
        {
            return type switch
            {
                LoginType.Email => new(value, LoginType.Email),
                LoginType.Phone => new(value, LoginType.Phone),
                _ => throw new DataInconsistencyDomainException(
                    ErrorCodes.GeneralError.OperationNotAllowed, 
                    $"Unknown login type. Entity {nameof(Login)}!"
                )
            };
        }

        public override string ToString() => Value;

        public bool IsEmail => Type == LoginType.Email;

        public bool IsPhone => Type == LoginType.Phone;

        public override bool Equals(object? obj)
        {
            if (obj is not Login other) return false;
            return Value == other.Value && Type == other.Type;
        }

        public override int GetHashCode() => HashCode.Combine(Value, Type);

        public static bool operator ==(Login? left, Login? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Login? left, Login? right) => !(left == right);
    }
}
