using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.ValueObjects.LoginUser
{
    /// <summary>
    /// Представляє логін користувача на основі телефонного номера.
    ///
    /// (Represents a user login based on a telephone number.)
    /// </summary>
    public sealed class PhoneLogin : Login
    {
        /// <summary>
        /// Телефонний номер, що використовується як логін користувача.
        ///
        /// (Telephone number used as the user login.)
        /// </summary>
        public Phone Phone { get; }

        internal PhoneLogin(Phone phone)
        {
            Phone = phone;
        }

        public override string Value => Phone.GetFullNumber();

        public override LoginType Type => LoginType.Phone;

        #region Restoration
        /// <summary>
        /// Відновлює телефонний логін зі збережених канонічних
        /// значень коду країни та національної частини.
        ///
        /// (Restores a telephone login from the persisted canonical
        /// country-code and national-number values.)
        /// </summary>
        /// <param name="storedCountryCode">
        /// Збережений міжнародний код країни.
        /// </param>
        /// <param name="storedNationalNumber">
        /// Збережена національна частина телефонного номера.
        /// </param>
        /// <returns>Відновлений телефонний логін.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо одна зі збережених складових порушує
        /// доменні правила або не відповідає канонічному формату.
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// правил валідації телефонного номера.
        /// </exception>
        public static PhoneLogin Restore(
            string storedCountryCode,
            string storedNationalNumber)
        {
            var code = PhoneCountryCode.Restore(storedCountryCode);
            var number = PhoneNationalNumber.Restore(storedNationalNumber);

            var phone = Phone.Restore(code, number);

            return new PhoneLogin(phone);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Phone;
        }
    }
}
