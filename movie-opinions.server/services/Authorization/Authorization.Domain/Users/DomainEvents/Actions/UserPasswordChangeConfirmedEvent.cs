using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    /// <summary>
    /// Доменна подія, що фіксує підтвердження зміни пароля користувача.
    ///
    /// (Domain event indicating that a change to the user's password was confirmed.)
    /// </summary>
    public sealed class UserPasswordChangeConfirmedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login Login { get; }

        /// <summary>
        /// Створює подію підтвердження зміни пароля користувача.
        ///
        /// (Creates an event representing confirmation of a user password change.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userPendingActionId">
        /// Ідентифікатор підтвердженої дії зі зміни пароля.
        ///
        /// (Identifier of the confirmed action to change the password.)
        /// </param>
        /// <param name="login">
        /// Логін користувача.
        ///
        /// (The user login.)
        /// </param>
        /// <param name="occurredOn">
        /// Час підтвердження зміни пароля.
        ///
        /// (The password change confirmation time.)
        /// </param>
        public UserPasswordChangeConfirmedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login login,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            Login = login;
        }
    }
}
