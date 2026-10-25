using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    /// <summary>
    /// Доменна подія, що фіксує запит на видалення
    /// облікового запису користувача.
    ///
    /// (Domain event indicating that deletion
    /// of a user account was requested.)
    /// </summary>
    public sealed class UserAccountDeletionRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login Login { get; }

        public ConfirmationFlowToken ConfirmationFlowToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Створює подію запиту на видалення облікового запису користувача.
        ///
        /// (Creates an event representing a request to delete the user's account.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userPendingActionId">
        /// Ідентифікатор відкладеної дії з видалення облікового запису.
        ///
        /// (Identifier of the pending action to delete the account.)
        /// </param>
        /// <param name="login">
        /// Логін користувача.
        ///
        /// (The user login.)
        /// </param>
        /// <param name="confirmationFlowToken">
        /// Токен потоку підтвердження видалення облікового запису.
        ///
        /// (The token identifying the account deletion confirmation flow.)
        /// </param>
        /// <param name="expiresAt">
        /// Час завершення періоду підтвердження дії.
        ///
        /// (The expiration time of the action confirmation period.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення запиту на видалення облікового запису.
        ///
        /// (The time when account deletion was requested.)
        /// </param>
        public UserAccountDeletionRequestedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login login,
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset expiresAt,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            Login = login;
            ConfirmationFlowToken = confirmationFlowToken;
            ExpiresAt = expiresAt;
        }
    }
}
