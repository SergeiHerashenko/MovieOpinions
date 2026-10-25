using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    /// <summary>
    /// Доменна подія, що фіксує запит на зміну пароля користувача.
    ///
    /// (Domain event indicating that a change to the user's password was requested.)
    /// </summary>
    public sealed class UserPasswordChangeRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login Login { get; }

        public ConfirmationFlowToken ConfirmationFlowToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Створює подію запиту на зміну пароля користувача.
        ///
        /// (Creates an event representing a request to change the user's password.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userPendingActionId">
        /// Ідентифікатор відкладеної дії зі зміни пароля.
        ///
        /// (Identifier of the pending action to change the password.)
        /// </param>
        /// <param name="login">
        /// Логін користувача.
        ///
        /// (The user login.)
        /// </param>
        /// <param name="confirmationFlowToken">
        /// Токен потоку підтвердження зміни пароля.
        ///
        /// (The token identifying the password change confirmation flow.)
        /// </param>
        /// <param name="expiresAt">
        /// Час завершення періоду підтвердження дії.
        ///
        /// (The expiration time of the action confirmation period.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення запиту на зміну пароля.
        ///
        /// (The time when the password change was requested.)
        /// </param>
        public UserPasswordChangeRequestedEvent(
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
