using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Deletion
{
    /// <summary>
    /// Доменна подія, що фіксує остаточне завершення процесу
    /// видалення облікового запису користувача.
    ///
    /// (Domain event indicating that deletion
    /// of a user account was finalized.)
    /// </summary>
    public sealed class UserAccountDeletionFinalizedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserDeletionId UserDeletionId { get; }

        public Login Login { get; }

        /// <summary>
        /// Створює подію остаточного завершення процесу
        /// видалення облікового запису користувача.
        ///
        /// (Creates an event representing the finalization
        /// of user account deletion.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userDeletionId">
        /// Ідентифікатор запису про видалення користувача.
        ///
        /// (The user deletion record identifier.)
        /// </param>
        /// <param name="login">
        /// Логін користувача.
        ///
        /// (The user login.)
        /// </param>
        /// <param name="occurredOn">
        /// Час фіксації остаточного завершення видалення.
        ///
        /// (The time when deletion finalization was recorded.)
        /// </param>
        public UserAccountDeletionFinalizedEvent(
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
