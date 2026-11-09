using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects
{
    /// <summary>
    /// Строго типізований ідентифікатор сутності UserDeletion.
    ///
    /// (Strongly typed identifier of the UserDeletion entity.)
    /// </summary>
    public sealed class UserDeletionId : StronglyTypedId<Guid>
    {
        /// <summary>
        /// Скалярне значення ідентифікатора.
        ///
        /// (Scalar value of the identifier.)
        /// </summary>
        public override Guid Value { get; }

        private UserDeletionId(Guid value)
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
        /// Новий ідентифікатор запису про видалення користувача.
        ///
        /// (The new user deletion record identifier.)
        /// </returns>
        internal static UserDeletionId Create()
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
        /// Відновлений ідентифікатор запису про видалення користувача.
        ///
        /// (The restored user deletion record identifier.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення дорівнює Guid.Empty.
        ///
        /// (Thrown when the persisted value is Guid.Empty.)
        /// </exception>
        public static UserDeletionId Restore(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw DomainDataInconsistencyException.Empty<UserDeletionId>(nameof(value));
            }

            return new(value);
        }
        #endregion

        /// <summary>
        /// Неявно перетворює ідентифікатор запису про видалення користувача
        /// на його значення Guid.
        ///
        /// (Implicitly converts a user deletion record identifier
        /// to its Guid value.)
        /// </summary>
        public static implicit operator Guid(UserDeletionId data)
            => data.Value;
    }
}
