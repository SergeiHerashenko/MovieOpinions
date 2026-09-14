namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Validation
{
    internal static partial class RestrictionRuleValidator
    {
        /// <summary>
        /// Містить незмінний набір вхідних значень, який спільно
        /// використовується правилами валідації RestrictionRule.
        ///
        /// (Contains an immutable set of input values shared
        /// by the RestrictionRule validation rules.)
        /// </summary>
        private sealed class RestrictionRuleValidationData
        {
            public required string Name { get; init; }

            public required int DurationMinutes { get; init; }
        }
    }
}
