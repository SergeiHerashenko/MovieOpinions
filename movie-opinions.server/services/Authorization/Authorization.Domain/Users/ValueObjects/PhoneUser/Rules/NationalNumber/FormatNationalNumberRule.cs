using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.NationalNumber
{
    public sealed class FormatNationalNumberRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private static readonly Regex AllowedSymbolsRegex =
            new(@"^[0-9\s\-\(\)]+$", RegexOptions.Compiled);

        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!AllowedSymbolsRegex.IsMatch(value))
            {
                return new ValidationRestoreFailure()
                {
                    Error = PhoneErrors.InvalidFormatNationalNumber<PhoneNationalNumber>(value),
                    BuildException = () => DomainDataInconsistencyException.InvalidFieldFormat<PhoneNationalNumber>(nameof(value), value)
                };
            }

            return null;
        }
    }
}
