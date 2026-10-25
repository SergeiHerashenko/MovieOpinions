using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;

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
        /// Створює подію додавання обмежень до наявної сесії обмежень користувача.
        ///
        /// (Creates an event representing the addition of restrictions
        /// to an existing user restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">
        /// Ідентифікатор оновленої сесії.
        ///
        /// (The updated session identifier.)
        /// </param>
        /// <param name="login">
        /// Логін користувача, якому належить сесія.
        ///
        /// (The login of the user who owns the session.)
        /// </param>
        /// <param name="restrictionDescriptions">
        /// Описи обмежень, доданих до сесії.
        ///
        /// (Descriptions of the restrictions added to the session.)
        /// </param>
        /// <param name="restrictionType">
        /// Тип обмежень оновленої сесії.
        ///
        /// (The restriction type of the updated session.)
        /// </param>
        /// <param name="totalBlockedMinutes">
        /// Загальна тривалість блокування у хвилинах після додавання обмежень.
        ///
        /// (The total blocked duration in minutes after the restrictions are added.)
        /// </param>
        /// <param name="occurredOn">
        /// Час додавання обмежень.
        ///
        /// (The time when the restrictions were added.)
        /// </param>
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
