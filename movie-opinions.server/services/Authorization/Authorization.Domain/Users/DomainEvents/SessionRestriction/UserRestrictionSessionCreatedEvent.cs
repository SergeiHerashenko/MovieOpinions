using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents.SessionRestriction
{
    /// <summary>
    /// Представляє факт створення нової сесії обмежень користувача.
    ///
    /// Містить опис початкових обмежень, тип сесії та її загальну
    /// тривалість після створення.
    ///
    /// (Represents the creation of a new user restriction session.
    ///
    /// Contains descriptions of the initial restrictions, the session type,
    /// and its total blocked duration after creation.)
    /// </summary>
    public sealed class UserRestrictionSessionCreatedEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> RestrictionDescriptions { get; }

        public RestrictionType RestrictionType { get; }

        public int TotalBlockedMinutes { get; }

        /// <summary>
        /// Створює подію створення нової сесії обмежень користувача.
        ///
        /// (Creates an event representing the creation
        /// of a new user restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">
        /// Ідентифікатор створеної сесії.
        ///
        /// (The created session identifier.)
        /// </param>
        /// <param name="login">
        /// Логін користувача, для якого створено сесію.
        ///
        /// (The login of the user for whom the session was created.)
        /// </param>
        /// <param name="restrictionDescriptions">
        /// Описи початкових обмежень сесії.
        ///
        /// (Descriptions of the initial session restrictions.)
        /// </param>
        /// <param name="restrictionType">
        /// Тип обмежень створеної сесії.
        ///
        /// (The restriction type of the created session.)
        /// </param>
        /// <param name="totalBlockedMinutes">
        /// Загальна тривалість блокування у хвилинах після створення сесії.
        ///
        /// (The total blocked duration in minutes after session creation.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення сесії.
        ///
        /// (The session creation time.)
        /// </param>
        public UserRestrictionSessionCreatedEvent(
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
