using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.UsersRestriction.ValueObjects
{
    public sealed class RestrictionRule : ValueObject
    {
        public string Name { get; }

        public int DurationDay { get; }

        private RestrictionRule(string name, int durationDay)
        {
            Name = name;
            DurationDay = durationDay;
        }

        public static Result<RestrictionRule> Create(string name, int durationDay)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<RestrictionRule>.Failure(RestrictionError.Empty(nameof(name), nameof(RestrictionRule)));

            if (durationDay <= 0)
                return Result<RestrictionRule>.Failure(RestrictionError.NotEnoughDays);

            return Result<RestrictionRule>.Success(new RestrictionRule(name, durationDay));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return DurationDay;
        }
    }
}
