using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Common.Validation;
using Authorization.Domain.Common.Errors.Users;
using FluentValidation;
using System.Linq.Expressions;

namespace Authorization.Application.Features.Registration.StartRegistration
{
    public static class ValidationExtensionsPassword
    {
        public static IRuleBuilderOptions<T, string> PasswordRules<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                    .WithDomainError(PasswordErrors.EmptyPlainPassword<T>())
                .MinimumLength(8)
                    .WithDomainError(PasswordErrors.TooShortPassword<T>())
                .MaximumLength(64)
                    .WithDomainError(PasswordErrors.TooLongPassword<T>())
                .Matches("[A-Z]")
                    .WithDomainError(PasswordErrors.MissingUppercaseLetterPlainPassword<T>())
                .Matches("[a-z]")
                    .WithDomainError(PasswordErrors.MissingLowercaseLetterPlainPassword<T>())
                .Matches("[0-9]")
                    .WithDomainError(PasswordErrors.MissingNumberPlainPassword<T>());
        }

        public static IRuleBuilderOptions<T, string> ConfirmPasswordRules<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> passwordPropertySelector)
        {
            return ruleBuilder
                .NotEmpty()
                    .WithApplicationError(RegistrationErrors.ConfirmPasswordRequired<T>())
                .Equal(passwordPropertySelector)
                    .WithApplicationError(RegistrationErrors.PasswordsDoNotMatch<T>());
        }

        public static IRuleBuilderOptions<T, bool> AcceptTermsRules<T>(
            this IRuleBuilder<T, bool> ruleBuilder)
        {
            return ruleBuilder
                .Equal(true)
                    .WithApplicationError(RegistrationErrors.TermsMustBeAccepted<T>());
        }
    }
}
