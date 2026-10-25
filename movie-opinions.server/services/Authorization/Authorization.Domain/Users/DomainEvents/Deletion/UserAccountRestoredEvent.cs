using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Deletion
{
    /// <summary>
    /// Доменна подія, що фіксує відновлення облікового запису користувача.
    ///
    /// (Domain event indicating that a user account was restored.)
    /// </summary>
    public sealed class UserAccountRestoredEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserDeletionId UserDeletionId { get; }

        public Login Login { get; }

        /// <summary>
        /// Створює подію відновлення облікового запису користувача.
        ///
        /// (Creates an event representing user account restoration.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор відновленого користувача.
        ///
        /// (The restored user identifier.)
        /// </param>
        /// <param name="userDeletionId">
        /// Ідентифікатор запису про видалення, для якого виконано відновлення.
        ///
        /// (Identifier of the deletion record associated with the restoration.)
        /// </param>
        /// <param name="login">
        /// Логін відновленого користувача.
        ///
        /// (The restored user login.)
        /// </param>
        /// <param name="occurredOn">
        /// Час відновлення облікового запису.
        ///
        /// (The account restoration time.)
        /// </param>
        public UserAccountRestoredEvent(
            UserId userId,
            UserDeletionId userDeletionId,
            Login login,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserDeletionId = userDeletionId;
            Login = login;
        }
    }
}
