using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    /// <summary>
    /// Представляє перевірений пароль у відкритому вигляді,
    /// призначений для короткочасної передачі до сервісу хешування.
    ///
    /// Значення не нормалізується, не підтримує відновлення
    /// та не повинно зберігатися або журналюватися.
    ///
    /// (Represents a validated plaintext password intended for
    /// short-lived transfer to the hashing service.
    ///
    /// The value is not normalized, cannot be restored,
    /// and must not be persisted or logged.)
    /// </summary>
    public sealed class PlainPassword : ValueObject
    {
        /// <summary>
        /// Точне перевірене значення пароля без нормалізації.
        ///
        /// (Exact validated password value without normalization.)
        /// </summary>
        public string Value { get; }

        private PlainPassword(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Перевіряє переданий пароль без зміни його початкового значення
        /// та створює короткочасне доменне представлення для хешування.
        ///
        /// (Validates the supplied password without modifying its original value
        /// and creates a transient domain representation for hashing.)
        /// </summary>
        /// <param name="password">Пароль у відкритому вигляді.</param>
        /// <returns>
        /// Успішний результат із PlainPassword або перше
        /// виявлене порушення правил пароля.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        /// </exception>
        public static Result<PlainPassword> Create(string password)
        {
            var failure = PlainPasswordValidator.ValidateForError(
                OperationType.Create,
                password
            );

            if (failure is not null)
                return Result<PlainPassword>.Failure(failure.Value);

            return Result<PlainPassword>.Success(new PlainPassword(password));
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
