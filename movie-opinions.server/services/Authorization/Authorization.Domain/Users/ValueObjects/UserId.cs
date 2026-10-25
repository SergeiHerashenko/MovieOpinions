using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.ValueObjects
{
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
        /// <returns>Новий ідентифікатор користувача.
        ///
        /// (The new user identifier.)
        /// </returns>
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
        /// <param name="value">
        /// Збережене значення ідентифікатора.
        ///
        /// (The stored identifier value.)
        /// </param>
        /// <returns>
        /// Відновлений ідентифікатор користувача.
        ///
        /// (The restored user identifier.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережений ідентифікатор порожній.
        ///
        /// (Thrown when the stored identifier is empty.)
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

        /// <summary>
        /// Неявно перетворює ідентифікатор користувача на його значення Guid.
        ///
        /// (Implicitly converts a user identifier to its Guid value.)
        /// </summary>
        public static implicit operator Guid(UserId data)
            => data.Value;
    }
}
