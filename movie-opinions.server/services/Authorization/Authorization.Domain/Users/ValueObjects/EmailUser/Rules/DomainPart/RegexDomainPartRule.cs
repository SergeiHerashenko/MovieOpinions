using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public sealed class RegexDomainPartRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private static readonly Regex DomainPartRegex =
            new(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DomainPartRegex.IsMatch(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.InvalidFormatDomainPart<EmailDomainPart>(value),
                BuildException = () => DomainDataInconsistencyException.InvalidFieldFormat<EmailDomainPart>(nameof(value), value)
            };
        }
    }
}
