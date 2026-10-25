using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects
{
    /// <summary>
    /// Строго типізований ідентифікатор сутності UserRestriction.
    ///
    /// (Strongly typed identifier of the UserRestriction entity.)
    /// </summary>
    public sealed class UserRestrictionId : StronglyTypedId<Guid>
    {
        /// <summary>
        /// Скалярне значення ідентифікатора.
        ///
        /// (Scalar value of the identifier.)
        /// </summary>
        public override Guid Value { get; }

        private UserRestrictionId(Guid value)
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
        /// Новий ідентифікатор обмеження користувача.
        ///
        /// (The new user restriction identifier.)
        /// </returns>
        internal static UserRestrictionId Create()
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
        /// Відновлений ідентифікатор обмеження користувача.
        ///
        /// (The restored user restriction identifier.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення дорівнює Guid.Empty.
        ///
        /// (Thrown when the persisted value is Guid.Empty.)
        /// </exception>
        public static UserRestrictionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRestrictionId>(nameof(value));

            return new(value);
        }
        #endregion

        /// <summary>
        /// Неявно перетворює ідентифікатор обмеження користувача
        /// на його значення Guid.
        ///
        /// (Implicitly converts a user restriction identifier
        /// to its Guid value.)
        /// </summary>
        public static implicit operator Guid(UserRestrictionId data)
            => data.Value;
    }
}
