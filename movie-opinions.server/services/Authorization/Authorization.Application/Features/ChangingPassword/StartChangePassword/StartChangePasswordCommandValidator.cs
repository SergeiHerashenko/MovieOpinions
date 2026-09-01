using Authorization.Application.Common.Validation;
using Authorization.Domain.Common.Errors.Users;
using FluentValidation;

namespace Authorization.Application.Features.ChangingPassword.StartChangePassword
{
    public sealed class StartChangePasswordCommandValidator
        : AbstractValidator<StartChangePasswordCommand>
    {
        public StartChangePasswordCommandValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Потрібно ввести поточний пароль!");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                    .WithDomainError(PasswordErrors.EmptyPlainPassword<StartChangePasswordCommandValidator>())
                    .WithMessage("Новий пароль є обов'язковим!")
                .MinimumLength(8).WithMessage("Новий пароль повинен містити мінімум 8 символів!")
                .Matches("[A-Z]").WithMessage("Новий пароль повинен містити хоча б одну велику літеру!")
                .Matches("[a-z]").WithMessage("Новий пароль повинен містити хоча б одну малу літеру!");
        }
    }
}
