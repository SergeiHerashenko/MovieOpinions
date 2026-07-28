using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;

namespace Authorization.Domain.Users.ValueObjects.LoginUser
{
    public sealed class EmailLogin : Login
    {
        public Email Email { get; }

        public EmailLogin(Email email)
        {
            Email = email;
        }

        public override string Value => Email.GetFullEmail();

        public override LoginType Type => LoginType.Email;

        #region Restoration
        public static EmailLogin Restore(string emailLocalPart, string emailDomainPart)
        {
            var localPart = EmailLocalPart.Restore(emailLocalPart);
            var domainPart = EmailDomainPart.Restore(emailDomainPart);

            var email = Email.Restore(localPart, domainPart);

            return new EmailLogin(email);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Email;
        }
    }
}
