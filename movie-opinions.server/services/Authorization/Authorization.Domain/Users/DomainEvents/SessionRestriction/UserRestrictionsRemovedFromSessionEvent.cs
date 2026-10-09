using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

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
        /// Створює подію відкликання частини обмежень із сесії.
        ///
        /// (Creates an event representing the revocation
        /// of some restrictions from a session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">Ідентифікатор оновленої сесії.</param>
        /// <param name="restrictionType">Тип оновленої сесії.</param>
        /// <param name="userRestrictions">Обмеження, відкликані із сесії.</param>
        /// <param name="login">Логін користувача, якому належить сесія.</param>
        /// <param name="totalBlockedMinutes">Загальна тривалість блокування після відкликання обмежень.</param>
        /// <param name="occurredOn">Час відкликання обмежень.</param>
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
