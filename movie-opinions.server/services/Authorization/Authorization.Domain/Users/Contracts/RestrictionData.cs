using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.Contracts
{
    public class RestrictionData
    {
        public required RestrictionType RestrictionType { get; set; }

        public required RestrictionRule RestrictionRule { get; set; }

        public required string RestrictedBy { get; set; }

        public string? Reason { get; set; }
    }
}
