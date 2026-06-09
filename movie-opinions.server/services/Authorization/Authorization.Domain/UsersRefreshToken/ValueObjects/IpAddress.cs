using Authorization.Domain.Common.Errors.TokenError;
using Authorization.Domain.Common.Exceptions;
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
                return Result<IpAddress>.Failure(IpError.InvalidFormat($"Invalid IP address: {value}"));

            return Result<IpAddress>.Success(new IpAddress(value));
        }

        public static IpAddress Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.EmptyOnRestore(
                "IP address is missing during restore",
                new Dictionary<string, object>
                {
                    ["entity"] = nameof(IpAddress),
                    ["field"] = nameof(value),
                    ["operation"] = "restore"
                });

            value = Normalize(value);

            if (!IsValidIPv4(value))
                throw DomainDataInconsistencyException.InvalidValue(
                    $"Corrupted IP address in persistence: {value}",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(IpAddress),
                        ["field"] = nameof(value),
                        ["operation"] = "restore"
                    }
                );

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
