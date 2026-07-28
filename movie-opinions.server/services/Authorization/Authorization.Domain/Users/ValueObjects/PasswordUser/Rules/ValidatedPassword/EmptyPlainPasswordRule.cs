using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword
{
    public sealed class EmptyPlainPasswordRule : IValidationRule<string, ValidationFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationFailure? Validate(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return null;

            return new ValidationFailure()
            {
                Error = PasswordErrors.EmptyPlainPassword<PlainPassword>()
            };
        }
    }
}
