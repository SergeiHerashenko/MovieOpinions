using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    /// <summary>
    /// Доменна подія, що фіксує запит на зміну логіна користувача.
    ///
    /// (Domain event indicating that a change to the user's login was requested.)
    /// </summary>
    public sealed class UserLoginChangeRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login CurrentLogin { get; }

        public Login RequestedLogin { get; }

        public ConfirmationFlowToken ConfirmationFlowToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Створює подію запиту на зміну логіна користувача.
        ///
        /// (Creates an event representing a request to change the user's login.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userPendingActionId">
        /// Ідентифікатор відкладеної дії зі зміни логіна.
        ///
        /// (Identifier of the pending action to change the login.)
        /// </param>
        /// <param name="currentLogin">
        /// Поточний логін користувача на момент запиту.
        ///
        /// (The user's current login at the time of the request.)
        /// </param>
        /// <param name="requestedLogin">
        /// Новий логін, на який користувач запитав зміну.
        ///
        /// (The new login requested by the user.)
        /// </param>
        /// <param name="confirmationFlowToken">
        /// Токен потоку підтвердження зміни логіна.
        ///
        /// (The token identifying the login change confirmation flow.)
        /// </param>
        /// <param name="expiresAt">
        /// Час завершення періоду підтвердження дії.
        ///
        /// (The expiration time of the action confirmation period.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення запиту на зміну логіна.
        ///
        /// (The time when the login change was requested.)
        /// </param>
        public UserLoginChangeRequestedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login currentLogin,
            Login requestedLogin,
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset expiresAt,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            CurrentLogin = currentLogin;
            RequestedLogin = requestedLogin;
            ConfirmationFlowToken = confirmationFlowToken;
            ExpiresAt = expiresAt;
        }
    }
}
