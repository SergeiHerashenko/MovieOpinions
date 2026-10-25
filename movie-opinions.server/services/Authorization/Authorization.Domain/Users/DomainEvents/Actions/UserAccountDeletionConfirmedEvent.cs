using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    /// <summary>
    /// Доменна подія, що фіксує підтвердження видалення
    /// облікового запису користувача.
    ///
    /// (Domain event indicating that deletion
    /// of a user account was confirmed.)
    /// </summary>
    public sealed class UserAccountDeletionConfirmedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login Login { get; }

        public DateTimeOffset RestoreUntil { get; }

        /// <summary>
        /// Створює подію підтвердження видалення облікового запису користувача.
        ///
        /// (Creates an event representing confirmation of user account deletion.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userPendingActionId">
        /// Ідентифікатор підтвердженої дії з видалення облікового запису.
        ///
        /// (Identifier of the confirmed action to delete the account.)
        /// </param>
        /// <param name="login">
        /// Логін користувача.
        ///
        /// (The user login.)
        /// </param>
        /// <param name="restoreUntil">
        /// Момент завершення періоду відновлення облікового запису.
        ///
        /// (The moment when the account restoration period ends.)
        /// </param>
        /// <param name="occurredOn">
        /// Час підтвердження видалення облікового запису.
        ///
        /// (The account deletion confirmation time.)
        /// </param>
        public UserAccountDeletionConfirmedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login login,
            DateTimeOffset restoreUntil,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            Login = login;
            RestoreUntil = restoreUntil;
        }
    }
}
