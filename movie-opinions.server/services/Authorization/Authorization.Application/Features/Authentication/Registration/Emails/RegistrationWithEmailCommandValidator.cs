using FluentValidation;

namespace Authorization.Application.Features.Authentication.Registration.Emails
{
    public class RegistrationWithEmailCommandValidator : AbstractValidator<RegistrationWithEmailCommand>
    {
        public RegistrationWithEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Логін є обов'язковим");

            RuleFor(x => x.Password).PasswordRules();

            RuleFor(x => x.ConfirmPassword).ConfirmPasswordRules(x => x.Password);

            RuleFor(x => x.AcceptTerms).AcceptTermsRules();
        }
    }
}
