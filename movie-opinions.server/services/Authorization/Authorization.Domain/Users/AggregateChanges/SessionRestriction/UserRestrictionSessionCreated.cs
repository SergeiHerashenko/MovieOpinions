using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    /// <summary>
    /// Фіксує створену сесію обмежень як зміну агрегату,
    /// яку потрібно зберегти.
    ///
    /// (Records a newly created restriction session as an aggregate
    /// change to be persisted.)
    /// </summary>
    public sealed class UserRestrictionSessionCreated : AggregateChange
    {
        public UserRestrictionSession UserRestrictionSession { get; }

        public UserRestrictionSessionCreated(
            UserRestrictionSession userRestrictionSession,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSession = userRestrictionSession;
        }
    }
}
