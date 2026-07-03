using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.EmailUser
{
    public sealed class Email : ValueObject
    {
        public string Value { get; }

        private static readonly Regex EmailRegex =
            new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const int MAX_LENGTH_EMAIL = 30;

        private Email(string value)
        {
            Value = value;
        }

        #region Creation
        public static Result<Email> Create(string rawEmail)
        {
            if (string.IsNullOrWhiteSpace(rawEmail))
                return Result<Email>.Failure(UserErrors.EmailError.EmptyEmail<Email>());

            var trimmed = rawEmail.Trim();

            if(trimmed.Length > MAX_LENGTH_EMAIL)
                return Result<Email>.Failure(UserErrors.EmailError.TooLong<Email>());

            if (!EmailRegex.IsMatch(trimmed))
                return Result<Email>.Failure(UserErrors.EmailError.InvalidFormat<Email>());

            var normalized = trimmed.ToLowerInvariant();

            return Result<Email>.Success(new Email(normalized));
        }
        #endregion

        #region Restoration
        public static Email Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<Email>(nameof(value));

            return new Email(value.ToLowerInvariant());
        }
        #endregion

        public override string ToString() => Value;

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
