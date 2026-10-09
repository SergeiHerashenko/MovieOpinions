using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.ValueObjects
{
    /// <summary>
    /// Представляє унікальний ідентифікатор агрегату користувача.
    ///
    /// (Represents the unique identifier of the user aggregate.)
    /// </summary>
    public sealed class UserId : AggregateRootId<Guid>
    {
        /// <summary>
        /// Скалярне значення ідентифікатора користувача.
        ///
        /// (Scalar value of the user identifier.)
        /// </summary>
        public override Guid Value { get; }

        private UserId(Guid value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Створює новий ідентифікатор користувача на основі UUID версії 7.
        ///
        /// (Creates a new user identifier based on a version 7 UUID.)
        /// </summary>
        /// <returns>Новий ідентифікатор користувача.</returns>
        internal static UserId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює ідентифікатор користувача зі збереженого значення.
        ///
        /// (Restores a user identifier from its persisted value.)
        /// </summary>
        /// <param name="value">Збережене значення ідентифікатора.</param>
        /// <returns>Відновлений ідентифікатор користувача.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережений ідентифікатор порожній.
        /// </exception>
        public static UserId Restore(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw DomainDataInconsistencyException.Empty<UserId>(
                    nameof(value),
                    OperationType.Restore
                );
            }
            
            return new(value);
        }
        #endregion

        public static implicit operator Guid(UserId data)
            => data.Value;
    }
}
