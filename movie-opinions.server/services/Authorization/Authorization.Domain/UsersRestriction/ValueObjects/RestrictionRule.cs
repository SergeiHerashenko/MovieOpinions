using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validations;
using Authorization.Domain.Results;

namespace Authorization.Domain.UsersRestriction.ValueObjects
{
    public sealed class RestrictionRule : ValueObject
    {
        public string Name { get; }

        public int DurationDays { get; }

        private RestrictionRule(string name, int durationDays)
        {
            Name = name;
            DurationDays = durationDays;
        }

        public static Result<RestrictionRule> Create(string name, int durationDays)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<RestrictionRule>.Failure(RestrictionError.Empty<RestrictionRule>(nameof(name)));

            if (durationDays <= 0)
                return Result<RestrictionRule>.Failure(RestrictionError.NotEnoughDays<RestrictionRule>());

            return Result<RestrictionRule>.Success(new RestrictionRule(name, durationDays));
        }

        public static RestrictionRule Restore(string name, int durationDays)
        {
            DomainGuard.AgainstNull<RestrictionRule>(
                (durationDays, nameof(durationDays))
            );

            if(string.IsNullOrWhiteSpace(name))
                throw DomainDataInconsistencyException.Empty<RestrictionRule>(nameof(name));

            if (durationDays <= 0)
                throw DomainDataInconsistencyException.InvalidFieldFormat<RestrictionRule>(nameof(durationDays), durationDays);

            return new(name, durationDays);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return DurationDays;
        }
    }
}
