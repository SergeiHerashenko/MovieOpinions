using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    /// <summary>
    /// Представляє незмінне доменне значення пароля,
    /// яке містить його хеш та підтримує перевірку відповідності
    /// через передану функцію.
    ///
    /// (Represents an immutable domain password value
    /// containing its hash and supporting matching
    /// through a supplied verification function.)
    /// </summary>
    public sealed class Password : ValueObject
    {
        private readonly PasswordHash _hash;

        private Password(PasswordHash hash)
        {
            _hash = hash;
        }

        /// <summary>
        /// Текстове значення хешу пароля.
        ///
        /// (String representation of the password hash.)
        /// </summary>
        public string Value => _hash.Value;

        #region Creation
        /// <summary>
        /// Створює доменне представлення пароля
        /// з переданого хешу.
        ///
        /// (Creates the domain representation of a password
        /// from the supplied hash.)
        /// </summary>
        /// <param name="hash">Хеш пароля, отриманий від сервісу хешування.</param>
        /// <returns>Створений Password.</returns>
        /// <remarks>
        /// Передавання null порушує контракт виклику
        /// та спричиняє виняток.
        ///
        /// (Passing null violates the method contract
        /// and causes an exception.)
        /// </remarks>
        public static Password Create(PasswordHash hash)
        {
            DomainGuard.AgainstNull<Password>(
                OperationType.Create,
                (hash, nameof(hash))
            );

            return new Password(hash);
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює доменне представлення пароля
        /// з попередньо відновленого хешу.
        ///
        /// (Restores the domain representation of a password
        /// from a previously restored hash.)
        /// </summary>
        /// <param name="hash">Хеш, відновлений зі збереженого значення.</param>
        /// <returns>Відновлений Password.</returns>
        /// <remarks>
        /// Передавання null порушує контракт виклику
        /// та спричиняє виняток.
        ///
        /// (Passing null violates the method contract
        /// and causes an exception.)
        /// </remarks>
        public static Password Restore(PasswordHash hash)
        {
            DomainGuard.AgainstNull<Password>(
                OperationType.Restore,
                (hash, nameof(hash))
            );

            return new Password(hash);
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Перевіряє відповідність переданого пароля у відкритому вигляді
        /// збереженому хешу через надану функцію перевірки.
        ///
        /// (Checks whether the supplied plaintext password matches
        /// the stored hash using the provided verification function.)
        /// </summary>
        /// <param name="plainPassword">
        /// Пароль у відкритому вигляді, перевірений доменними правилами.
        /// </param>
        /// <param name="verifier">
        /// Функція, яка перевіряє відповідність пароля переданому хешу.
        /// </param>
        /// <returns>
        /// true, якщо пароль відповідає хешу; інакше false.
        /// </returns>
        /// <remarks>
        /// Обидва аргументи є обов’язковими; передавання null
        /// спричиняє виняток.
        /// Винятки функції перевірки передаються виклику без перехоплення.
        ///
        /// (Both arguments are required; passing null causes an exception.
        /// Exceptions from the verification function propagate
        /// to the caller without being caught.)
        /// </remarks>
        internal bool Matches(
            PlainPassword plainPassword,
            Func<PlainPassword, PasswordHash, bool> verifier)
        {
            DomainGuard.AgainstNull<Password>(
                OperationType.Update,
                (plainPassword, nameof(plainPassword)),
                (verifier, nameof(verifier))
            );

            return verifier(plainPassword, _hash);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _hash.Value;
        }
    }
}
