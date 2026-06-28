using FluentValidation;
using System.Linq.Expressions;

namespace Authorization.Application.Features.Authentication.Registration
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> PasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
            .NotEmpty().WithMessage("Пароль є обов'язковим!")
            .MinimumLength(8).WithMessage("Пароль повинен містити мінімум 8 символів!")
            .Matches("[A-Z]").WithMessage("Пароль повинен містити хоча б одну велику літеру!")
            .Matches("[a-z]").WithMessage("Пароль повинен містити хоча б одну малу літеру!");
        }

        public static IRuleBuilderOptions<T, string> ConfirmPasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Expression<Func<T, string>> passwordPropertySelector)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Підтвердження пароля є обов'язковим!")
                .Equal(passwordPropertySelector).WithMessage("Паролі не співпадають!");
        }

        public static IRuleBuilderOptions<T, bool> AcceptTermsRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
        {
            return ruleBuilder
                .Equal(true).WithMessage("Ви повинні погодитися з правилами сервісу");
        }
    }
}
