using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects
{
    /// <summary>
    /// Строго типізований ідентифікатор дочірньої
    /// сутності UserRefreshToken.
    ///
    /// (Strongly typed identifier of the UserRefreshToken child entity.)
    /// </summary>
    public sealed class UserRefreshTokenId : StronglyTypedId<Guid>
    {
        /// <summary>
        /// Скалярне значення ідентифікатора.
        ///
        /// (Scalar value of the identifier.)
        /// </summary>
        public override Guid Value { get; }

        private UserRefreshTokenId(Guid value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює новий ідентифікатор на основі UUID версії 7.
        ///
        /// (Creates a new identifier based on a version 7 UUID.)
        /// </summary>
        /// <returns>
        /// Новий ідентифікатор refresh-токена користувача.
        ///
        /// (The new user refresh token identifier.)
        /// </returns>
        internal static UserRefreshTokenId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює ідентифікатор зі збереженого скалярного значення.
        ///
        /// (Restores the identifier from a persisted scalar value.)
        /// </summary>
        /// <param name="value">
        /// Збережене значення ідентифікатора.
        ///
        /// (The persisted identifier value.)
        /// </param>
        /// <returns>
        /// Відновлений ідентифікатор refresh-токена користувача.
        ///
        /// (The restored user refresh token identifier.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо значення дорівнює Guid.Empty.
        ///
        /// (Thrown when the persisted value is Guid.Empty.)
        /// </exception>
        public static UserRefreshTokenId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRefreshTokenId>(nameof(value));

            return new(value);
        }
        #endregion

        /// <summary>
        /// Неявно перетворює ідентифікатор refresh-токена користувача
        /// на його значення Guid.
        ///
        /// (Implicitly converts a user refresh token identifier
        /// to its Guid value.)
        /// </summary>
        public static implicit operator Guid(UserRefreshTokenId data)
            => data.Value;
    }
}
