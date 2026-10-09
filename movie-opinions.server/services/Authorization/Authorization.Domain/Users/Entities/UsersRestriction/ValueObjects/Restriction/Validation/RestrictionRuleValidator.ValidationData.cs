using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Validation
{
    internal static partial class RestrictionRuleValidator
    {
        /// <summary>
        /// Містить значення RestrictionRule та контекст доменної операції,
        /// що передаються повному набору правил валідації.
        ///
        /// (Contains RestrictionRule values and domain-operation context
        /// supplied to the complete validation-rule set.)
        /// </summary>
        private sealed class RestrictionRuleValidationData : IHasOperationType
        {
            public required OperationType OperationType { get; init; }

            public required string Name { get; init; }

            public required int DurationMinutes { get; init; }
        }
    }
}
