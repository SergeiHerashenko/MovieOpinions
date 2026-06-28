using FluentValidation;

namespace Authorization.Application.Features.Authentication.Registration.Phones
{
    public class RegistrationWithPhoneCommandValidator : AbstractValidator<RegistrationWithPhoneCommand>
    {
        public RegistrationWithPhoneCommandValidator()
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Код країни є обов'язковим для заповнення!");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефону є обов'язковим для заповнення!");

            RuleFor(x => x.Password).PasswordRules();

            RuleFor(x => x.ConfirmPassword).ConfirmPasswordRules(x => x.Password);

            RuleFor(x => x.AcceptTerms).AcceptTermsRules();
        }
    }
}
