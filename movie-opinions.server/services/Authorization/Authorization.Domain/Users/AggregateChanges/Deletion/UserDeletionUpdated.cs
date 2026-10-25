using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion;

namespace Authorization.Domain.Users.AggregateChanges.Deletion
{
    /// <summary>
    /// Фіксує оновлений запис про видалення користувача
    /// як зміну агрегату, яку потрібно зберегти.
    ///
    /// (Records an updated user deletion record
    /// as an aggregate change to be persisted.)
    /// </summary>
    public sealed class UserDeletionUpdated : AggregateChange
    {
        public UserDeletion UserDeletion { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження
        /// оновленого запису про видалення користувача.
        ///
        /// (Creates an aggregate change for persisting
        /// an updated user deletion record.)
        /// </summary>
        /// <param name="userDeletion">
        /// Оновлений запис про видалення користувача.
        /// (Updated user deletion record.)
        /// </param>
        /// <param name="occurredOn">
        /// Час оновлення запису про видалення.
        /// (Time the deletion record was updated.)
        /// </param>
        public UserDeletionUpdated(
            UserDeletion userDeletion,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserDeletion = userDeletion;
        }
    }
}
