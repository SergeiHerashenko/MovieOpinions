using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.LocalPart
{
    public sealed class LocalPartRegexRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private static readonly Regex LocalPartRegex =
            new(@"^[a-zA-Z0-9](?!(?:.*[._-]){2})(?:[a-zA-Z0-9._-]*[a-zA-Z0-9])?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (LocalPartRegex.IsMatch(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.InvalidFormatLocalPart<EmailLocalPart>(value),
                BuildException = () => DomainDataInconsistencyException.InvalidFieldFormat<EmailLocalPart>(nameof(value), value)
            };
        }
    }
}
