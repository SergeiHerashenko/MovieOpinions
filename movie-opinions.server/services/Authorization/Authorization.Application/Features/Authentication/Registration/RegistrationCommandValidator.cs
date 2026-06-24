using FluentValidation;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationCommandValidator : AbstractValidator<RegistrationCommand>
    {
        public RegistrationCommandValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Логін є обов'язковим");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль є обов'язковим")
                .MinimumLength(8).WithMessage("Пароль повинен містити мінімум 8 символів")
                .Matches("[A-Z]").WithMessage("Пароль повинен містити хоча б одну велику літеру")
                .Matches("[a-z]").WithMessage("Пароль повинен містити хоча б одну малу літеру");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Підтвердження пароля є обов'язковим")
                .Equal(x => x.Password).WithMessage("Паролі не співпадають");

            RuleFor(x => x.AcceptTerms)
                .Equal(true).WithMessage("Ви повинні погодитися з правилами сервісу");
        }
    }
}
