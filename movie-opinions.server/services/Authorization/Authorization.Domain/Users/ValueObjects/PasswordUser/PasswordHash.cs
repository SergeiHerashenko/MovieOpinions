using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    /// <summary>
    /// Представляє незмінне текстове значення хешу пароля.
    /// Забезпечує наявність щонайменше одного непробільного символу.
    ///
    /// (Represents an immutable textual password-hash value.
    /// Ensures that it contains at least one non-whitespace character.)
    /// </summary>
    /// <remarks>
    /// Формат і походження хешу цим типом не перевіряються.
    ///
    /// (The hash format and origin are not verified by this type.)
    /// </remarks>
    public sealed class PasswordHash : ValueObject
    {
        /// <summary>
        /// Текстове значення хешу, збережене без змін.
        ///
        /// (Textual hash value preserved without modification.)
        /// </summary>
        public string Value { get; }

        private PasswordHash(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює PasswordHash із результату хешування
        /// та перевіряє наявність значення.
        ///
        /// (Creates PasswordHash from a hashing result
        /// and validates that the value is present.)
        /// </summary>
        /// <param name="value">Хеш пароля, отриманий від сервісу хешування.</param>
        /// <returns>Створений PasswordHash.</returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо значення є null, порожнім
        /// або складається лише з пробільних символів.
        /// Це порушує передумову отримання непорожнього результату хешування.
        /// </exception>
        public static PasswordHash Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainInvalidOperationException.PreconditionFailed<PasswordHash>(
                    nameof(Create),
                    "A non-empty password hash.",
                    OperationType.Create,
                    context: new Dictionary<string, object>
                    {
                        ["FieldName"] = nameof(Value)
                    }
                );
            }

            return new PasswordHash(value);
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює PasswordHash зі збереженого значення
        /// та перевіряє його наявність.
        ///
        /// (Restores PasswordHash from a persisted value
        /// and validates that the value is present.)
        /// </summary>
        /// <param name="value">Збережене текстове значення хешу пароля.</param>
        /// <returns>Відновлений PasswordHash.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення є null, порожнім
        /// або складається лише з пробільних символів.
        /// </exception>
        public static PasswordHash Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.Empty<PasswordHash>(
                    nameof(Value),
                    OperationType.Restore
                );
            }

            return new PasswordHash(value);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
