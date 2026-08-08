using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.Emails
{
    public class SingleAtSignEmailRule : IValidationRule<string, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(string value)
        {
            var first = value.IndexOf('@');
            var last = value.LastIndexOf('@');

            if (first <= 0 || first != last || first == value.Length - 1)
            {
                return new ValidationRestoreFailure()
                {
                    Error = EmailErrors.InvalidFormatEmail<Email>(value),
                    BuildException = () => DomainDataInconsistencyException.InvalidFieldFormat<Email>(nameof(value), value)
                };
            }

            return null;
        }
    }
}
