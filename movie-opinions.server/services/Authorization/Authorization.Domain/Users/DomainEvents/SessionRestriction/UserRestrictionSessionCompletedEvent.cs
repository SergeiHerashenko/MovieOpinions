using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents.SessionRestriction
{
    /// <summary>
    /// Представляє факт завершення сесії обмежень за часовими правилами.
    ///
    /// Подія виникає, коли сесія вже сплила або стала простроченою
    /// після перерахунку її загальної тривалості.
    ///
    /// (Represents the completion of a restriction session
    /// according to its time-based rules.
    ///
    /// The event occurs when the session has already expired or becomes
    /// expired after its total duration has been recalculated.)
    /// </summary>
    public sealed class UserRestrictionSessionCompletedEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public RestrictionType RestrictionType { get; }

        /// <summary>
        /// Створює подію завершення сесії обмежень користувача.
        ///
        /// (Creates an event representing the completion
        /// of a user restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">
        /// Ідентифікатор завершеної сесії.
        ///
        /// (The completed session identifier.)
        /// </param>
        /// <param name="login">
        /// Логін користувача, якому належала сесія.
        ///
        /// (The login of the user who owned the session.)
        /// </param>
        /// <param name="restrictionType">
        /// Тип обмежень завершеної сесії.
        ///
        /// (The restriction type of the completed session.)
        /// </param>
        /// <param name="occurredOn">
        /// Час фіксації завершення сесії.
        ///
        /// (The time when session completion was recorded.)
        /// </param>
        public UserRestrictionSessionCompletedEvent(
            UserRestrictionSessionId userRestrictionSessionId,
            Login login,
            RestrictionType restrictionType,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
            Login = login;
            RestrictionType = restrictionType;
        }
    }
}
