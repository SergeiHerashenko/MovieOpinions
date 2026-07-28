using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.ValueObjects.EmailUser
{
    public sealed class Email : ValueObject
    {
        public EmailLocalPart EmailLocalPart { get; }

        public EmailDomainPart EmailDomainPart { get; }

        private Email(
            EmailLocalPart emailLocalPart, 
            EmailDomainPart emailDomainPart)
        {
            EmailLocalPart = emailLocalPart;
            EmailDomainPart = emailDomainPart;
        }


        #region Creation
        public static Result<Email> Create(EmailLocalPart emailLocalPart, EmailDomainPart emailDomainPart)
        {
            if (emailLocalPart is null)
                return Result<Email>.Failure(EmailErrors.EmptyLocalPart<Email>());

            if (emailDomainPart is null)
                return Result<Email>.Failure(EmailErrors.EmptyDomainPart<Email>());

            return Result<Email>.Success(new Email(emailLocalPart, emailDomainPart));
        }
        #endregion

        #region Restoration
        public static Email Restore(EmailLocalPart emailLocalPart, EmailDomainPart emailDomainPart)
        {
            DomainGuard.AgainstNull<Email>(
                (emailLocalPart, nameof(emailLocalPart)),
                (emailDomainPart, nameof(emailDomainPart))
            );

            return new Email(emailLocalPart, emailDomainPart);
        }
        #endregion

        public string GetFullEmail()
        {
            return $"{EmailLocalPart.Value}{EmailDomainPart.Value}";
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return EmailLocalPart;
            yield return EmailDomainPart;
        }
    }
}
