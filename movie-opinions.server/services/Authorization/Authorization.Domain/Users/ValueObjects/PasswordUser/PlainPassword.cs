using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    public sealed class PlainPassword : ValueObject
    {
        public string Value { get; }

        private PlainPassword(string value)
        {
            Value = value;
        }

        private static readonly ValidationOrchestrator<string, ValidationFailure> _validator = new(
            [
                new EmptyPlainPasswordRule(),
                new NoLowLetterPlainPasswordRule(),
                new NoNumberPlainPasswordRule(),
                new NoUpperLetterPlainPasswordRule(),
                new TooLongPlainPasswordRule(),
                new TooShortPlainPassword()
            ]
        );

        #region Creation
        public static Result<PlainPassword> Create(string password)
        {
            var failure = _validator.Validate(password);

            if (failure is not null)
                return Result<PlainPassword>.Failure(failure.Error);

            return Result<PlainPassword>.Success(new PlainPassword(password));
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
