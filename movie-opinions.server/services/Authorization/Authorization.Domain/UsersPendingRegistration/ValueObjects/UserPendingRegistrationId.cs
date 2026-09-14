using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    /// <summary>
    /// Строго типізований ідентифікатор агрегату UserPendingRegistration.
    ///
    /// (Strongly typed identifier of the UserPendingRegistration aggregate.)
    /// </summary>
    public sealed class UserPendingRegistrationId : AggregateRootId<Guid>
    {
        public override Guid Value { get; }

        private UserPendingRegistrationId(Guid value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює новий ідентифікатор на основі UUID версії 7.
        ///
        /// (Creates a new identifier based on a version 7 UUID.)
        /// </summary>
        public static UserPendingRegistrationId Create()
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
        /// <param name="value">Збережене значення ідентифікатора.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення дорівнює Guid.Empty.
        /// </exception>
        public static UserPendingRegistrationId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserPendingRegistrationId>(nameof(value));

            return new(value);
        }
        #endregion

        /// <summary>
        /// Повертає скалярне значення строго типізованого ідентифікатора.
        ///
        /// (Returns the scalar value of the strongly typed identifier.)
        /// </summary>
        public static implicit operator Guid(UserPendingRegistrationId data)
            => data.Value;
    }
}
