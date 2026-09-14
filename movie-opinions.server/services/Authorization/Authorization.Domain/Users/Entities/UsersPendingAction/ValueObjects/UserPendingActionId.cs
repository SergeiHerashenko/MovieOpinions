using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects
{
    /// <summary>
    /// Строго типізований ідентифікатор дочірньої сутності UserPendingAction.
    ///
    /// (Strongly typed identifier of the UserPendingAction child entity.)
    /// </summary>
    public sealed class UserPendingActionId : StronglyTypedId<Guid>
    {
        /// <summary>
        /// Скалярне значення ідентифікатора.
        ///
        /// (Scalar value of the identifier.)
        /// </summary>
        public override Guid Value { get; }

        private UserPendingActionId(Guid value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює новий ідентифікатор на основі UUID версії 7.
        ///
        /// (Creates a new identifier based on a version 7 UUID.)
        /// </summary>
        internal static UserPendingActionId Create()
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
        /// <param name="value">Збережене значення ідентифікатора.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо значення дорівнює Guid.Empty.
        /// </exception>
        public static UserPendingActionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserPendingActionId>(nameof(value));

            return new(value);
        }
        #endregion

        /// <summary>
        /// Повертає скалярне значення строго типізованого ідентифікатора.
        ///
        /// (Returns the scalar value of the strongly typed identifier.)
        /// </summary>
        public static implicit operator Guid(UserPendingActionId data)
            => data.Value;
    }
}
