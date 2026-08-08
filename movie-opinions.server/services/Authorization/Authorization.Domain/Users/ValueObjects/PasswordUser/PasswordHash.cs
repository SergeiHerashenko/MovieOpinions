using Authorization.Domain.Common.Models;
using System.Text.Json.Serialization;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    public sealed class PasswordHash : ValueObject
    {
        public string Value { get; }

        [JsonConstructor]
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
