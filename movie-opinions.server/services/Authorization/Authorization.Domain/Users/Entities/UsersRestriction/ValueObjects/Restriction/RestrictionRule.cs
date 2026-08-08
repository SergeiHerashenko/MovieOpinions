using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Rules;
using System.Text.Json.Serialization;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction
{
    public sealed class RestrictionRule : ValueObject
    {
        public string Name { get; }

        public int DurationMinute { get; }

        [JsonConstructor]
        private RestrictionRule(string name, int durationMinute )
        {
            Name = name;
            DurationMinute = durationMinute;
        }

        private static readonly ValidationOrchestrator<RestrictionRuleValidationData, ValidationRestoreFailure> _validator = new(
            [
                new EmptyRestrictionRule(),
                new MinDurationMinutesRule()
            ]
        );

        #region Creation
        internal static Result<RestrictionRule> Create(string name, int durationMinute)
        {
            var failure = _validator.Validate(BuildValidationData(name, durationMinute));

            if (failure is not null)
                return Result<RestrictionRule>.Failure(failure.Error);

            return Result<RestrictionRule>.Success(new RestrictionRule(name, durationMinute));
        }
        #endregion

        #region Restoration
        public static RestrictionRule Restore(string name, int durationMinute)
        {
            var failure = _validator.Validate(BuildValidationData(name, durationMinute));

            if (failure is not null)
                throw failure.BuildException();

            return new RestrictionRule(name, durationMinute);
        }
        #endregion

        private static RestrictionRuleValidationData BuildValidationData(string name, int durationMinute)
        {
            return new()
            {
                Name = name,
                DurationMinutes = durationMinute
            };
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return DurationMinute;
        }
    }
}
