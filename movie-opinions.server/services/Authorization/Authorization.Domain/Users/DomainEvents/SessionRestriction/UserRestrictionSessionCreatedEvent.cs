using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

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
        /// Створює подію створення нової сесії обмежень.
        ///
        /// (Creates an event representing the creation
        /// of a new restriction session.)
        /// </summary>
        /// <param name="userRestrictionSessionId">Ідентифікатор створеної сесії.</param>
        /// <param name="login">Логін користувача, для якого створено сесію.
        /// <param name="restrictionDescriptions">Описи початкових обмежень сесії.</param>
        /// <param name="restrictionType">Тип створеної сесії.</param>
        /// <param name="totalBlockedMinutes">Загальна тривалість блокування після створення сесії.</param>
        /// <param name="occurredOn">Час створення сесії.</param>
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
