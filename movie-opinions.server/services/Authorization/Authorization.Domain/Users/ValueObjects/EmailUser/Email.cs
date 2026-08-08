using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.Emails;
using System.Text.Json.Serialization;

namespace Authorization.Domain.Users.ValueObjects.EmailUser
{
    public sealed class Email : ValueObject
    {
        public EmailLocalPart EmailLocalPart { get; }

        public EmailDomainPart EmailDomainPart { get; }

        [JsonConstructor]
        private Email(
            EmailLocalPart emailLocalPart, 
            EmailDomainPart emailDomainPart)
        {
            EmailLocalPart = emailLocalPart;
            EmailDomainPart = emailDomainPart;
        }

        private static readonly ValidationOrchestrator<string, ValidationRestoreFailure> _validator = new(
            [
                new EmptyEmailRule(),
                new SingleAtSignEmailRule()
            ]
        );

        #region Creation
        public static Result<Email> Create(string rawEmail)
        {
            var failure = _validator.Validate(rawEmail);

            if (failure is not null)
                return Result<Email>.Failure(failure.Error);

            var (localPart, domainPart) = Split(rawEmail);

            var localResult = EmailLocalPart.Create(localPart);

            if (localResult.IsFailure)
                return Result<Email>.Failure(localResult.Errors);

            var domainResult = EmailDomainPart.Create(domainPart);

            if(domainResult.IsFailure)
                return Result<Email>.Failure(domainResult.Errors);

            return Result<Email>.Success(new Email(localResult.Value, domainResult.Value));
        }
        #endregion

        #region Restoration
        public static Email Restore(string rawEmail)
        {
            var failure = _validator.Validate(rawEmail);

            if (failure is not null)
                throw failure.BuildException();

            var (localPart, domainPart) = Split(rawEmail);

            var localResult = EmailLocalPart.Restore(localPart);

            var domainResult = EmailDomainPart.Restore(domainPart);

            return new Email(localResult, domainResult);
        }
        #endregion

        /// <summary>
        /// Splits validated email into local and domain parts.
        /// Caller must ensure the email has already passed validation.
        /// </summary>
        private static (string LocalPart, string DomainPart) Split(string email)
        {
            var separatorIndex = email.IndexOf('@');

            return (
                email[..separatorIndex],
                email[(separatorIndex + 1)..]
            );
        }

        public string GetFullEmail()
        {
            return $"{EmailLocalPart.Value}@{EmailDomainPart.Value}";
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return EmailLocalPart;
            yield return EmailDomainPart;
        }
    }
}
