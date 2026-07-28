using Authorization.Domain.Users.Enums;
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

        public override LoginType  Type => LoginType.Phone;

        #region Restoration
        public static PhoneLogin Restore(string countryCode, string nationalNumber)
        {
            var code = PhoneCountryCode.Restore(countryCode);
            var number = PhoneNationalNumber.Restore(nationalNumber);

            var phone = Phone.Restore(code, number);

            return new PhoneLogin(phone);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
