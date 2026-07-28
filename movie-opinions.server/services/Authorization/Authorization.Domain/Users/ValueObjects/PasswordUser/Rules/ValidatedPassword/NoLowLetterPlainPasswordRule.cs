using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword
{
    public sealed class NoLowLetterPlainPasswordRule : IValidationRule<string, ValidationFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Any(char.IsLower))
                return null;

            return new ValidationFailure()
            {
                Error = PasswordErrors.MissingLowercaseLetterPlainPassword<PlainPassword>()
            };
        }
    }
}
