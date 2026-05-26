using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PhoneUser;

namespace Authorization.Domain.Users.ValueObjects.LoginUser
{
    public sealed class PhoneLogin : Login
    {
        public Phone Phone { get; }

        public PhoneLogin(Phone phone)
        {
            Phone = phone;
        }

        public override string Value => Phone.GetFullNumber();

        public override LoginType Type => LoginType.Phone;

        public static PhoneLogin Restore(string countryCode, string phoneNumber)
        {
            var phone = Phone.Restore(CountryCode.Create(countryCode).Value, phoneNumber);

            return new PhoneLogin(phone);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Phone;
        }
    }
}
