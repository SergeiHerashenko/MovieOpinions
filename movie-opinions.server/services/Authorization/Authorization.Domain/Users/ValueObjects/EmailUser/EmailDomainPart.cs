using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart;

namespace Authorization.Domain.Users.ValueObjects.EmailUser
{
    public sealed class EmailDomainPart : ValueObject
    {
        public string Value { get; }

        private EmailDomainPart(string value)
        {
            Value = value;
        }

        private static readonly ValidationOrchestrator<string, ValidationRestoreFailure> _validator = new(
            [
                new EmptyDomainPartRule(),
                new TooLongDomainPartRule(),
                new TooShortDomainPartRule(),
                new RegexDomainPartRule(),
                new NotAllowedDomainPartRule()
            ]
        );

        #region Creation
        public static Result<EmailDomainPart> Create(string rawEmailDomain)
        {
            var failure = _validator.Validate(rawEmailDomain);

            if (failure is not null)
                return Result<EmailDomainPart>.Failure(failure.Error);

            var normalized = rawEmailDomain.Trim().ToLowerInvariant();

            return Result<EmailDomainPart>.Success(new EmailDomainPart(normalized));
        }
        #endregion

        #region Restoration
        public static EmailDomainPart Restore(string rawEmailDomain)
        {
            var failure = _validator.Validate(rawEmailDomain);

            if (failure is not null)
                throw failure.BuildException();

            var normalized = rawEmailDomain.Trim().ToLowerInvariant();

            return new EmailDomainPart(normalized);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
