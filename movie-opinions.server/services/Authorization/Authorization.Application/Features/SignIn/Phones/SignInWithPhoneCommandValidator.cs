using FluentValidation;

namespace Authorization.Application.Features.SignIn.Phones
{
    public class SignInWithPhoneCommandValidator : AbstractValidator<SignInWithPhoneCommand>
    {
        public SignInWithPhoneCommandValidator()
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Код країни є обов'язковим");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефону є обов'язковим");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль є обов'язковим!");
        }
    }
}
