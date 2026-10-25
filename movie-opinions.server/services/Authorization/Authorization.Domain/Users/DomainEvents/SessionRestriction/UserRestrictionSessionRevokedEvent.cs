using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents.SessionRestriction
{
    /// <summary>
    /// Представляє факт повного ручного відкликання
    /// сесії обмежень користувача.
    ///
    /// Фіксує відкликання всіх обмежень сесії
    /// та вилучення сесії з активної колекції
    /// сесій агрегату User.
    ///
    /// (Represents the complete manual revocation
    /// of a user's restriction session.
    ///
    /// Records the revocation of all restrictions in the session
    /// and the removal of the session from the User aggregate's
    /// active session collection.)
    /// </summary>
    public sealed class UserRestrictionSessionRevokedEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public RestrictionType RestrictionType { get; }

        /// <summary>
        /// Створює подію повного відкликання сесії обмежень користувача.
        ///
        /// (Creates an event representing the complete revocation
        /// of a user restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">
        /// Ідентифікатор відкликаної сесії.
        ///
        /// (The revoked session identifier.)
        /// </param>
        /// <param name="login">
        /// Логін користувача, якому належала сесія.
        ///
        /// (The login of the user who owned the session.)
        /// </param>
        /// <param name="restrictionType">
        /// Тип обмежень відкликаної сесії.
        ///
        /// (The restriction type of the revoked session.)
        /// </param>
        /// <param name="occurredOn">
        /// Час відкликання сесії.
        ///
        /// (The session revocation time.)
        /// </param>
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
