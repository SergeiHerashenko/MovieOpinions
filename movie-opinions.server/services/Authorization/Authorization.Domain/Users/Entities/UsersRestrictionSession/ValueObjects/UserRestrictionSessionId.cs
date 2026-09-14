using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects
{
    /// <summary>
    /// Строго типізований ідентифікатор сесії обмежень користувача.
    ///
    /// (Strongly typed identifier of a user restriction session.)
    /// </summary>
    public sealed class UserRestrictionSessionId : StronglyTypedId<Guid>
    {
        /// <summary>
        /// Скалярне значення ідентифікатора.
        ///
        /// (Scalar value of the identifier.)
        /// </summary>
        public override Guid Value { get; }

        private UserRestrictionSessionId(Guid value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює новий ідентифікатор на основі UUID версії 7.
        ///
        /// (Creates a new identifier based on a version 7 UUID.)
        /// </summary>
        internal static UserRestrictionSessionId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює ідентифікатор зі збереженого скалярного значення.
        ///
        /// (Restores the identifier from its persisted scalar value.)
        /// </summary>
        /// <param name="value">Збережене значення ідентифікатора.</param>
        /// <returns>Відновлений ідентифікатор сесії обмежень.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення дорівнює Guid.Empty.
        /// </exception>
        public static UserRestrictionSessionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRestrictionSessionId>(nameof(value));

            return new(value);
        }
        #endregion

        public static implicit operator Guid(UserRestrictionSessionId data)
            => data.Value;
    }
}
