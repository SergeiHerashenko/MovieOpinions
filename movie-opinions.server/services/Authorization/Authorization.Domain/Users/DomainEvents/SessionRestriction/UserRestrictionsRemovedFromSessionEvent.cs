using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents.SessionRestriction
{
    /// <summary>
    /// Представляє факт відкликання частини обмежень
    /// із наявної сесії користувача.
    ///
    /// Сесія після операції продовжує існувати з іншими
    /// активними обмеженнями.
    ///
    /// (Represents the revocation of some restrictions
    /// from an existing user restriction session.
    ///
    /// After the operation, the session continues to exist
    /// with other active restrictions.)
    /// </summary>
    public sealed class UserRestrictionsRemovedFromSessionEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public RestrictionType RestrictionType { get; }

        public IReadOnlyCollection<UserRestriction> UserRestrictions { get; }

        public Login Login { get; }

        public int TotalBlockedMinutes { get; }

        /// <summary>
        /// Створює подію відкликання частини обмежень
        /// із наявної сесії обмежень користувача.
        ///
        /// (Creates an event representing the revocation of some restrictions
        /// from an existing user restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">
        /// Ідентифікатор оновленої сесії.
        ///
        /// (The updated session identifier.)
        /// </param>
        /// <param name="restrictionType">
        /// Тип обмежень оновленої сесії.
        ///
        /// (The restriction type of the updated session.)
        /// </param>
        /// <param name="userRestrictions">
        /// Обмеження, відкликані із сесії.
        ///
        /// (The restrictions revoked from the session.)
        /// </param>
        /// <param name="login">
        /// Логін користувача, якому належить сесія.
        ///
        /// (The login of the user who owns the session.)
        /// </param>
        /// <param name="totalBlockedMinutes">
        /// Загальна тривалість блокування у хвилинах після відкликання обмежень.
        ///
        /// (The total blocked duration in minutes after the restrictions are revoked.)
        /// </param>
        /// <param name="occurredOn">
        /// Час відкликання обмежень.
        ///
        /// (The restriction revocation time.)
        /// </param>
        public UserRestrictionsRemovedFromSessionEvent(
            UserRestrictionSessionId userRestrictionSessionId,
            RestrictionType restrictionType,
            IReadOnlyCollection<UserRestriction> userRestrictions,
            Login login,
            int totalBlockedMinutes,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
            RestrictionType = restrictionType;
            UserRestrictions = userRestrictions;
            Login = login;
            TotalBlockedMinutes = totalBlockedMinutes;
        }
    }
}
