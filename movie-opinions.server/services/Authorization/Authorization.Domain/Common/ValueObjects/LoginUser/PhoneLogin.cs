using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Common.ValueObjects.LoginUser
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

        /// <summary>
        /// Ініціалізує телефонний логін із перевіреного телефонного номера.
        ///
        /// (Initializes a telephone login from a validated telephone number.)
        /// </summary>
        /// <param name="phone">
        /// Перевірений телефонний номер.
        ///
        /// (The validated telephone number.)
        /// </param>
        internal PhoneLogin(Phone phone)
        {
            Phone = phone;
        }

        /// <inheritdoc/>
        public override string Value => Phone.GetFullNumber();

        /// <inheritdoc/>
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
        ///
        /// (The persisted international country code.)
        /// </param>
        /// <param name="storedNationalNumber">
        /// Збережена національна частина телефонного номера.
        ///
        /// (The persisted national part of the telephone number.)
        /// </param>
        /// <returns>
        /// Відновлений телефонний логін.
        ///
        /// (The restored telephone login.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережені складові або їхнє поєднання
        /// порушують структурні правила чи канонічний формат номера.
        ///
        /// (Thrown when a persisted component or the component combination
        /// violates structural rules or the canonical telephone-number format.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// правил валідації телефонного номера.
        ///
        /// (Thrown when an internal precondition of the
        /// telephone-number validation rules is violated.)
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

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Phone;
        }
    }
}
