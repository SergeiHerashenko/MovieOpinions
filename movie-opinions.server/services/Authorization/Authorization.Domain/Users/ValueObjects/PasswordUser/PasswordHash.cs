using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    public sealed class PasswordHash : ValueObject
    {
        public string Value { get; }

        public PasswordHash(string value)
        {
            Value = value;
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
