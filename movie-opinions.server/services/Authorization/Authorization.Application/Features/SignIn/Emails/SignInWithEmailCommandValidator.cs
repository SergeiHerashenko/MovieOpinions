using FluentValidation;

namespace Authorization.Application.Features.SignIn.Emails
{
    public class SignInWithEmailCommandValidator : AbstractValidator<SignInWithEmailCommand>
    {
        public SignInWithEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Логін є обов'язковим");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль є обов'язковим!");
        }
    }
}
