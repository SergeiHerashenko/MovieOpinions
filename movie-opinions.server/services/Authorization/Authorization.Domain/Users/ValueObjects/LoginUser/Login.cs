using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;

namespace Authorization.Domain.Users.ValueObjects.LoginUser
{
    public abstract class Login : ValueObject
    {
        public abstract string Value { get; }

        public abstract LoginType Type { get; }

        public static Login From(Email email) => new EmailLogin(email);

        public static Login From(Phone phone) => new PhoneLogin(phone);

        public static Login Restore(string value, LoginType type, string? countryCode = null)
        {
            return type switch
            {
                LoginType.Email => EmailLogin.Restore(value),
                LoginType.Phone => PhoneLogin.Restore(countryCode!, value),
                _ => throw DomainDataInconsistencyException.UnsupportedDiscriminator<Login>(nameof(value), type.ToString())
            };
        }
    }
}
