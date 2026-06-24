using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
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
        public static DomainResult<Email> Create(string rawEmail)
        {
            if (string.IsNullOrWhiteSpace(rawEmail))
                return DomainResult<Email>.Failure(EmailError.Empty);

            var trimmed = rawEmail.Trim();

            if(trimmed.Length > MAX_LENGTH_EMAIL)
                return DomainResult<Email>.Failure(EmailError.TooLong);

            if (!EmailRegex.IsMatch(trimmed))
                return DomainResult<Email>.Failure(EmailError.InvalidFormat);

            var normalized = trimmed.ToLowerInvariant();

            return DomainResult<Email>.Success(new Email(normalized));
        }
        #endregion

        #region Restoration
        public static Email Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.EmptyOnRestore<Email>(
                    nameof(value)
                );
            }

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
