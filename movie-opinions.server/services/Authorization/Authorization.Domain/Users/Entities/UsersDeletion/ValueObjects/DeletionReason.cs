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
        /// Порожнє значення замінюється стандартною причиною.
        ///
        /// (Creates a deletion reason from the provided text.
        /// An empty value is replaced with the default reason.)
        /// </summary>
        internal static Result<DeletionReason> Create(string? rawReason)
        {
            var normalizedReason = string.IsNullOrWhiteSpace(rawReason)
                ? DEFAULT_REASON
                : rawReason.Trim();

            if (normalizedReason.Length > MAX_REASON_LENGTH)
                return Result<DeletionReason>.Failure(UserDeletionErrors.TooLongReason<DeletionReason>());

            return Result<DeletionReason>.Success(new DeletionReason(normalizedReason));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює причину видалення зі збереженого значення.
        ///
        /// (Restores a deletion reason from its persisted value.)
        /// </summary>
        /// <param name="value">Збережений текст причини видалення.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення порожнє або перевищує допустиму довжину.
        /// </exception>
        public static DeletionReason Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<DeletionReason>(nameof(value));

            var normalizedReason = value.Trim();

            if (normalizedReason.Length > MAX_REASON_LENGTH)
                throw DomainDataInconsistencyException.ValueOutOfRange<DeletionReason>(nameof(value));

            return new(normalizedReason);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
