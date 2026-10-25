using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    /// <summary>
    /// Доменна подія, що фіксує підтвердження зміни логіна користувача.
    ///
    /// (Domain event indicating that a change to the user's login was confirmed.)
    /// </summary>
    public sealed class UserLoginChangeConfirmedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login PreviousLogin { get; }

        public Login CurrentLogin { get; }

        /// <summary>
        /// Створює подію підтвердження зміни логіна користувача.
        ///
        /// (Creates an event representing confirmation of a user login change.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="userPendingActionId">
        /// Ідентифікатор підтвердженої дії зі зміни логіна.
        ///
        /// (Identifier of the confirmed action to change the login.)
        /// </param>
        /// <param name="previousLogin">
        /// Логін користувача до зміни.
        ///
        /// (The user login before the change.)
        /// </param>
        /// <param name="currentLogin">
        /// Логін користувача після підтвердження зміни.
        ///
        /// (The user login after the change was confirmed.)
        /// </param>
        /// <param name="occurredOn">
        /// Час підтвердження зміни логіна.
        ///
        /// (The login change confirmation time.)
        /// </param>
        public UserLoginChangeConfirmedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login previousLogin,
            Login currentLogin,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            PreviousLogin = previousLogin;
            CurrentLogin = currentLogin;
        }
    }
}
