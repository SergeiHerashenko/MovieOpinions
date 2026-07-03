using Authorization.Domain.Common.Errors.TokenError;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.UsersRefreshToken.ValueObjects
{
    public sealed class IpAddress : ValueObject
    {
        public string Value { get; }

        private const int IPv4OctetCount = 4;

        private IpAddress(string value)
        {
            Value = value;
        }

        public static Result<IpAddress> Create(string value)
        {
            value = Normalize(value);

            if (!IsValidIPv4(value))
                return Result<IpAddress>.Failure(IpError.InvalidFormat<IpAddress>(value));

            return Result<IpAddress>.Success(new IpAddress(value));
        }

        public static IpAddress Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<IpAddress>(nameof(value));

            value = Normalize(value);

            if (!IsValidIPv4(value))
                throw DomainDataInconsistencyException.InvalidFieldFormat<IpAddress>(nameof(value), value);

            return new IpAddress(value);
        }

        private static bool IsValidIPv4(string value)
        {
            var parts = value.Split('.');

            if(parts.Length != IPv4OctetCount)
                return false;

            foreach(var part in parts)
            {
                if(!byte.TryParse(part, out _))
                    return false;
            }

            return true;
        }

        private static string Normalize(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
