using Authorization.Domain.Results;
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

        public override string Value => Email.Value;

        public override LoginType Type => LoginType.Email;

        public static EmailLogin Restore(string value)
        {
            return new EmailLogin(Email.Restore(value));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Email;
        }
    }
}
