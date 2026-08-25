using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Application.Common.Errors.Users
{
    public static class ContactErrors
    {
        public static Error ContactInvariantViolated<TValue>(string value)
            => new(ApplicationErrorCodes.ContactError.ContactInvariantViolated,
                   $"There are no communication channels for {value}. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvariantViolation
            );
    }
}
