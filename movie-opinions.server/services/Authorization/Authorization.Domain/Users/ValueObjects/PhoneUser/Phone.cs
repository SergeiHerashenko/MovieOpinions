using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.Phones;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
{
    /// <summary>
    /// Представляє повний телефонний номер, сформований
    /// із міжнародного коду країни та національної частини.
    ///
    /// Обидві складові зберігаються у канонічному форматі,
    /// тому повне значення не містить символів форматування.
    ///
    /// (Represents a complete telephone number composed of
    /// an international country code and a national number.
    ///
    /// Both components are stored in canonical form, so the complete
    /// value contains no formatting characters.)
    /// </summary>
    public sealed class Phone : ValueObject
    {
        /// <summary>
        /// Міжнародний телефонний код країни.
        ///
        /// (International telephone country code.)
        /// </summary>
        public PhoneCountryCode PhoneCountryCode { get; }

        /// <summary>
        /// Канонічна національна частина телефонного номера.
        ///
        /// (Canonical national part of the telephone number.)
        /// </summary>
        public PhoneNationalNumber PhoneNationalNumber { get; }

        private Phone(
            PhoneCountryCode phoneCountryCode,
            PhoneNationalNumber phoneNationalNumber)
        {
            PhoneCountryCode = phoneCountryCode;
            PhoneNationalNumber = phoneNationalNumber;
        }

        #region Creation
        /// <summary>
        /// Об’єднує попередньо створені складові телефонного номера,
        /// перевіряє їхню структурну узгодженість і застосовує
        /// політики, що діють під час створення.
        ///
        /// Метод призначений для внутрішньої композиції через LoginUser.
        ///
        /// (Combines previously created telephone-number components,
        /// validates their structural consistency, and applies policies
        /// enforced during creation.
        ///
        /// The method is intended for internal composition through LoginUser.)
        /// </summary>
        /// <param name="phoneCountryCode">
        /// Перевірений міжнародний код країни.
        /// </param>
        /// <param name="phoneNationalNumber">
        /// Перевірена національна частина номера.
        /// </param>
        /// <returns>
        /// Успішний результат із повним телефонним номером
        /// або перша виявлена помилка політики.
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо одна зі складових відсутня.
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        /// </exception>
        public static Result<Phone> Create(
            PhoneCountryCode phoneCountryCode,
            PhoneNationalNumber phoneNationalNumber)
        {
            DomainGuard.AgainstNull<Phone>(
                OperationType.Create,
                (phoneCountryCode, nameof(phoneCountryCode)),
                (phoneNationalNumber, nameof(phoneNationalNumber))
            );

            var validationPhone = PhoneValidator.ValidateForError(
                OperationType.Create,
                phoneCountryCode.Value,
                phoneNationalNumber.Value
            );

            if (validationPhone is not null)
                return Result<Phone>.Failure(validationPhone.Value);

            return Result<Phone>.Success(new Phone(phoneCountryCode, phoneNationalNumber));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює повний телефонний номер із попередньо
        /// відновлених складових.
        ///
        /// Під час відновлення перевіряється структурна узгодженість,
        /// але актуальні політики створення не застосовуються.
        ///
        /// (Restores a complete telephone number from previously
        /// restored components.
        ///
        /// Structural consistency is validated during restoration,
        /// while current creation policies are not applied.)
        /// </summary>
        /// <param name="phoneCountryCode">
        /// Відновлений міжнародний код країни.
        /// </param>
        /// <param name="phoneNationalNumber">
        /// Відновлена національна частина номера.
        /// </param>
        /// <returns>Відновлений телефонний номер.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо складові відсутні або утворюють
        /// структурно некоректний стан.
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        /// </exception>
        public static Phone Restore(
            PhoneCountryCode phoneCountryCode,
            PhoneNationalNumber phoneNationalNumber)
        {
            DomainGuard.AgainstNull<Phone>(
                OperationType.Restore,
                (phoneCountryCode, nameof(phoneCountryCode)),
                (phoneNationalNumber, nameof(phoneNationalNumber))
            );

            var validationPhone = PhoneValidator.ValidateForException(
                OperationType.Restore,
                phoneCountryCode.Value,
                phoneNationalNumber.Value
            );

            if (validationPhone is not null)
                throw validationPhone.Value;

            return new Phone(phoneCountryCode, phoneNationalNumber);
        }
        #endregion

        /// <summary>
        /// Повертає повний канонічний телефонний номер,
        /// об’єднуючи код країни та національну частину.
        ///
        /// (Returns the complete canonical telephone number by combining
        /// the country code and national part.)
        /// </summary>
        /// <returns>Повний телефонний номер у канонічному форматі.</returns>
        public string GetFullNumber()
        {
            return $"{PhoneCountryCode.Value}{PhoneNationalNumber.Value}";
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return PhoneCountryCode;
            yield return PhoneNationalNumber;
        }
    }
}
