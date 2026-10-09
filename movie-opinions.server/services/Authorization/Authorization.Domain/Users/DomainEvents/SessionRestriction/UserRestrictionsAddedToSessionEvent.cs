using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents.SessionRestriction
{
    /// <summary>
    /// Представляє факт додавання нових обмежень
    /// до наявної сесії користувача.
    ///
    /// Містить описи доданих обмежень і загальну тривалість
    /// сесії після її оновлення.
    ///
    /// (Represents the addition of new restrictions
    /// to an existing user restriction session.
    ///
    /// Contains descriptions of the added restrictions and the session’s
    /// total blocked duration after the update.)
    /// </summary>
    public sealed class UserRestrictionsAddedToSessionEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> RestrictionDescriptions { get; }

        public RestrictionType RestrictionType { get; }

        public int TotalBlockedMinutes { get; }

        /// <summary>
        /// Створює подію додавання обмежень до наявної сесії.
        ///
        /// (Creates an event representing the addition
        /// of restrictions to an existing session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">Ідентифікатор оновленої сесії.</param>
        /// <param name="login">Логін користувача, якому належить сесія.</param>
        /// <param name="restrictionDescriptions">Описи обмежень, доданих до сесії.</param>
        /// <param name="restrictionType">Тип оновленої сесії.</param>
        /// <param name="totalBlockedMinutes">Загальна тривалість блокування після додавання обмежень.</param>
        /// <param name="occurredOn">Час додавання обмежень.</param>
        public UserRestrictionsAddedToSessionEvent(
            UserRestrictionSessionId userRestrictionSessionId,
            Login login,
            IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> restrictionDescriptions,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
            Login = login;
            RestrictionDescriptions = restrictionDescriptions;
            RestrictionType = restrictionType;
            TotalBlockedMinutes = totalBlockedMinutes;
        }
    }
}
