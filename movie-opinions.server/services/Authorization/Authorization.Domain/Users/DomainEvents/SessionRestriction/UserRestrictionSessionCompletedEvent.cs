using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

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
        /// Створює подію завершення сесії обмежень.
        ///
        /// (Creates an event representing the completion
        /// of a restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">Ідентифікатор завершеної сесії.</param>
        /// <param name="login">Логін користувача, якому належала сесія.</param>
        /// <param name="restrictionType">Тип завершеної сесії.</param>
        /// <param name="occurredOn">Час фіксації завершення сесії.</param>
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
