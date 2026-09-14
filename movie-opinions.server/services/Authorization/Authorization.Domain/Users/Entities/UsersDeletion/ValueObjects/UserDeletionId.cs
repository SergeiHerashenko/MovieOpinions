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
        internal static UserDeletionId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює ідентифікатор із збереженого скалярного значення.
        ///
        /// (Restores the identifier from a persisted scalar value.)
        /// </summary>
        /// <param name="value">
        /// Збережене значення ідентифікатора.
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення дорівнює Guid.Empty.
        /// </exception>
        public static UserDeletionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserDeletionId>(nameof(value));

            return new(value);
        }
        #endregion

        public static implicit operator Guid(UserDeletionId data)
            => data.Value;
    }
}
