using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.ValueObjects.PasswordUser
{
    /// <summary>
    /// Представляє перевірений пароль у відкритому вигляді,
    /// призначений для короткочасного використання під час хешування
    /// або перевірки пароля.
    ///
    /// Значення не нормалізується, не підтримує відновлення
    /// та не повинно зберігатися або журналюватися.
    ///
    /// (Represents a validated plaintext password intended for
    /// short-lived use during hashing or password verification.
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
        /// та створює короткочасне доменне представлення для хешування або перевірки пароля.
        ///
        /// (Validates the supplied password without modifying its original value
        /// and creates a transient domain representation for hashing or password verification.)
        /// </summary>
        /// <param name="password">
        /// Пароль у відкритому вигляді.
        ///
        /// (The plaintext password.)
        /// </param>
        /// <returns>
        /// Успішний результат із PlainPassword або перше
        /// виявлене порушення правил пароля.
        ///
        /// (A successful result containing PlainPassword,
        /// or the first detected password-rule violation.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static Result<PlainPassword> Create(string password)
        {
            var failure = PlainPasswordValidator.ValidateForError(
                OperationType.Create,
                password
            );

            if (failure is not null)
            {
                return Result<PlainPassword>.Failure(failure.Value);
            }

            return Result<PlainPassword>.Success(new PlainPassword(password));
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
