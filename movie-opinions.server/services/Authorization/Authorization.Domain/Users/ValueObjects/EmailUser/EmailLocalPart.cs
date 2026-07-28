using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.LocalPart;

namespace Authorization.Domain.Users.ValueObjects.EmailUser
{
    public sealed class EmailLocalPart : ValueObject
    {
        public string Value { get; }

        private EmailLocalPart(string value)
        {
            Value = value;
        }

        private static readonly ValidationOrchestrator<string, ValidationRestoreFailure> _validator = new(
            [
                new EmptyLocalPartRule(),
                new TooLongLocalPartRule(),
                new TooShortLocalPartRule(),
                new LocalPartRegexRule()
            ]
        );

        #region Creation
        public static Result<EmailLocalPart> Create(string rawEmailLocalPart)
        {
            var failure = _validator.Validate(rawEmailLocalPart);

            if (failure is not null)
                return Result<EmailLocalPart>.Failure(failure.Error);

            var normalized = rawEmailLocalPart.Trim().ToLowerInvariant();

            return Result<EmailLocalPart>.Success(new EmailLocalPart(normalized));
        }
        #endregion

        #region Restoration
        public static EmailLocalPart Restore(string rawEmailLocalPart)
        {
            var failure = _validator.Validate(rawEmailLocalPart);

            if (failure is not null)
                throw failure.BuildException();

            var normalized = rawEmailLocalPart.Trim().ToLowerInvariant();

            return new EmailLocalPart(normalized);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
