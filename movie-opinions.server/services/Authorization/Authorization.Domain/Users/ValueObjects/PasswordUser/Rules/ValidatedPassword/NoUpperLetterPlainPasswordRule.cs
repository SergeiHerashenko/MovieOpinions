using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword
{
    public sealed class NoUpperLetterPlainPasswordRule : IValidationRule<string, ValidationFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Any(char.IsUpper))
                return null;

            return new ValidationFailure()
            {
                Error = PasswordErrors.MissingUppercaseLetterPlainPassword<PlainPassword>()
            };
        }
    }
}
