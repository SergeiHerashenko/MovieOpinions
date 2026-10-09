using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    /// <summary>
    /// Фіксує оновлену сесію обмежень як зміну агрегату,
    /// яку потрібно зберегти.
    ///
    /// (Records an updated restriction session as an aggregate
    /// change to be persisted.)
    /// </summary>
    public sealed class UserRestrictionSessionUpdated : AggregateChange
    {
        public UserRestrictionSession UserRestrictionSession { get; }

        public UserRestrictionSessionUpdated(
            UserRestrictionSession userRestrictionSession,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSession = userRestrictionSession;
        }
    }
}
