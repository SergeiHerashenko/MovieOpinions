using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Rules;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses
{
    public sealed class IpAddress : ValueObject
    {
        public string Value { get; }

        private IpAddress(string value)
        {
            Value = value;
        }

        private static readonly ValidationOrchestrator<string, ValidationRestoreFailure> _validator = new(
            [
                new EmptyIpAddressRule(),
                new ValidIpAddressRule()
            ]
        );

        #region Creation
        public static Result<IpAddress> Create(string value)
        {
            value = value.Trim();

            var failure = _validator.Validate(value);

            if (failure is not null)
                return Result<IpAddress>.Failure(failure.Error);

            return Result<IpAddress>.Success(new IpAddress(value)); 
        }
        #endregion

        #region Restoration
        public static IpAddress Restore(string value)
        {
            var failure = _validator.Validate(value);

            if (failure is not null)
                throw failure.BuildException();

            return new IpAddress(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
