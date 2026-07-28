namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Rules
{
    public sealed class RestrictionRuleValidationData
    {
        public string Name { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }
    }
}
