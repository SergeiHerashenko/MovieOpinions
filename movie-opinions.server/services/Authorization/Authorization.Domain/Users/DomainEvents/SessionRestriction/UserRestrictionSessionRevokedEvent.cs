using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents.SessionRestriction
{
    /// <summary>
    /// Представляє факт повного ручного скасування сесії обмежень.
    ///
    /// Подія виникає, коли всі обмеження сесії були відкликані,
    /// унаслідок чого сесія стала порожньою та припинила існування.
    ///
    /// (Represents the complete manual revocation of a restriction session.
    ///
    /// The event occurs when all restrictions in the session have been
    /// revoked, causing the session to become empty and cease to exist.)
    /// </summary>
    public sealed class UserRestrictionSessionRevokedEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public RestrictionType RestrictionType { get; }

        /// <summary>
        /// Створює подію повного скасування сесії обмежень.
        ///
        /// (Creates an event representing complete revocation
        /// of a restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">Ідентифікатор скасованої сесії.</param>
        /// <param name="login">Логін користувача, якому належала сесія.</param>
        /// <param name="restrictionType">Тип обмежень скасованої сесії.</param>
        /// <param name="occurredOn">Час скасування сесії.</param>
        public UserRestrictionSessionRevokedEvent(
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
