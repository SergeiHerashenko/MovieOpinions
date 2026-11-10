using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser
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
        /// (Combines previously created telephone-number components,
        /// validates their structural consistency, and applies policies
        /// enforced during creation.)
        /// </summary>
        /// <param name="phoneCountryCode">
        /// Перевірений міжнародний код країни.
        ///
        /// (The validated international country code.)
        /// </param>
        /// <param name="phoneNationalNumber">
        /// Перевірена національна частина номера.
        ///
        /// (The validated national part of the number.)
        /// </param>
        /// <returns>
        /// Успішний результат із повним телефонним номером
        /// або перша виявлена помилка структурної валідації чи політики.
        ///
        /// (A successful result containing the complete telephone
        /// number, or the first structural-validation or policy error.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо одна зі складових відсутня.
        ///
        /// (Thrown when either component is null.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// У каталозі телефонної нумерації знайдено записи
        /// з переданим міжнародним кодом, але їхній об’єднаний
        /// набір допустимих довжин національної частини порожній.
        ///
        /// Це означає порушення внутрішньої узгодженості каталогу.
        ///
        /// (The phone numbering plan catalog contains entries
        /// matching the supplied country calling code, but their
        /// combined set of allowed national number lengths is empty.
        ///
        /// This indicates an internal consistency violation in the catalog.)
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
            {
                return Result<Phone>.Failure(validationPhone.Value);
            }

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
        ///
        /// (The restored international country code.)
        /// </param>
        /// <param name="phoneNationalNumber">
        /// Відновлена національна частина номера.
        ///
        /// (The restored national part of the number.)
        /// </param>
        /// <returns>
        /// Відновлений телефонний номер.
        ///
        /// (The restored telephone number.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо складові відсутні або утворюють
        /// структурно некоректний стан.
        ///
        /// (Thrown when components are null or form a structurally invalid state.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// У каталозі телефонної нумерації знайдено записи
        /// з переданим міжнародним кодом, але їхній об’єднаний
        /// набір допустимих довжин національної частини порожній.
        ///
        /// Це означає порушення внутрішньої узгодженості каталогу.
        ///
        /// (The phone numbering plan catalog contains entries
        /// matching the supplied country calling code, but their
        /// combined set of allowed national number lengths is empty.
        ///
        /// This indicates an internal consistency violation in the catalog.)
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
            {
                throw validationPhone.Value;
            }

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
        /// <returns>
        /// Повний телефонний номер у канонічному форматі.
        ///
        ///(The complete telephone number in canonical form.)
        /// </returns>
        public string GetFullNumber()
        {
            return $"{PhoneCountryCode.Value}{PhoneNationalNumber.Value}";
        }

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return PhoneCountryCode;
            yield return PhoneNationalNumber;
        }
    }
}
