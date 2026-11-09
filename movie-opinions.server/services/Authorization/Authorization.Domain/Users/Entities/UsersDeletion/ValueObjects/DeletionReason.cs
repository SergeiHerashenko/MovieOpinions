using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersDeletion.Errors;

namespace Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects
{
    /// <summary>
    /// Представляє нормалізовану причину видалення користувача.
    /// Якщо під час створення причина не вказана, використовується стандартний текст.
    ///
    /// (Represents a normalized user deletion reason.
    /// If no reason is provided during creation, a default value is used.)
    /// </summary>
    public sealed class DeletionReason : ValueObject
    {
        private const int MAX_REASON_LENGTH = 600;
        private const string DEFAULT_REASON = "No deletion reason was provided.";

        /// <summary>
        /// Нормалізований непорожній текст причини видалення.
        ///
        /// (Normalized non-empty deletion reason.)
        /// </summary>
        public string Value { get; }

        private DeletionReason(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює причину видалення з отриманого тексту.
        ///
        /// Значення null, порожній або пробільний текст замінюється
        /// стандартною причиною. Для іншого тексту видаляються зовнішні
        /// пробільні символи та перевіряється допустима довжина.
        ///
        /// (Creates a deletion reason from the supplied text.
        ///
        /// A null, empty, or whitespace-only value is replaced with the default
        /// reason. Otherwise, surrounding whitespace is removed
        /// and the permitted length is validated.)
        /// </summary>
        /// <param name="rawReason">
        /// Зовнішній текст причини видалення або null, якщо причину не вказано.
        ///
        /// (The external deletion reason text, or null when no reason is provided.)
        /// </param>
        /// <returns>
        /// Успішний результат із нормалізованою причиною або невдалий
        /// результат із помилкою перевищення допустимої довжини.
        ///
        /// (A successful result containing the normalized reason,
        /// or a failed result indicating that the permitted length was exceeded.)
        /// </returns>
        internal static Result<DeletionReason> Create(string? rawReason)
        {
            var normalizedReason = string.IsNullOrWhiteSpace(rawReason)
                ? DEFAULT_REASON
                : rawReason.Trim();

            if (normalizedReason.Length > MAX_REASON_LENGTH)
            {
                return Result<DeletionReason>.Failure(UserDeletionErrors.TooLongReason<DeletionReason>());
            }

            return Result<DeletionReason>.Success(new DeletionReason(normalizedReason));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює причину видалення зі збереженого тексту,
        /// видаляє зовнішні пробільні символи та перевіряє допустиму довжину.
        ///
        /// (Restores a deletion reason from persisted text,
        /// removes surrounding whitespace, and validates the permitted length.)
        /// </summary>
        /// <param name="value">
        /// Збережений текст причини видалення.
        ///
        /// (The persisted deletion reason text.)
        /// </param>
        /// <returns>
        /// Відновлена нормалізована причина видалення.
        ///
        /// (The restored normalized deletion reason.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення відсутнє, порожнє
        /// або складається лише з пробільних символів,
        /// чи нормалізований текст перевищує допустиму довжину.
        ///
        /// (Thrown when the persisted value is missing, empty,
        /// or contains only whitespace, or when the normalized text
        /// exceeds the permitted length.)
        /// </exception>
        public static DeletionReason Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.Empty<DeletionReason>(nameof(value));
            }

            var normalizedReason = value.Trim();

            if (normalizedReason.Length > MAX_REASON_LENGTH)
            {
                throw DomainDataInconsistencyException.ValueOutOfRange<DeletionReason>(nameof(value));
            }

            return new(normalizedReason);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
