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
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Назва правила обмеження, передана для перевірки.
            ///
            /// (The restriction rule name supplied for validation.)
            /// </summary>
            public required string Name { get; init; }

            /// <summary>
            /// Тривалість обмеження у хвилинах, передана для перевірки.
            ///
            /// (The restriction duration in minutes supplied for validation.)
            /// </summary>
            public required int DurationMinutes { get; init; }
        }
    }
}
