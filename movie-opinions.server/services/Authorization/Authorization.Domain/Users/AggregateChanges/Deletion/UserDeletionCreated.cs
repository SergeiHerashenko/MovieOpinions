using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion;

namespace Authorization.Domain.Users.AggregateChanges.Deletion
{
    /// <summary>
    /// Фіксує новостворений запис про видалення користувача
    /// як зміну агрегату, яку потрібно зберегти.
    ///
    /// (Records a newly created user deletion record
    /// as an aggregate change to be persisted.)
    /// </summary>
    public sealed class UserDeletionCreated : AggregateChange
    {
        public UserDeletion UserDeletion { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження
        /// нового запису про видалення користувача.
        ///
        /// (Creates an aggregate change for persisting
        /// a new user deletion record.)
        /// </summary>
        /// <param name="userDeletion">
        /// Створений запис про видалення користувача.
        /// (Created user deletion record.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення запису про видалення.
        /// (Time the deletion record was created.)
        /// </param>
        public UserDeletionCreated(
            UserDeletion userDeletion,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserDeletion = userDeletion;
        }
    }
}
