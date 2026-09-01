using Authorization.Application.Common.Validation;
using Authorization.Domain.Common.Errors.Users;
using FluentValidation;

namespace Authorization.Application.Features.Registration.StartRegistration.Emails
{
    public class RegistrationWithEmailCommandValidator : AbstractValidator<RegistrationWithEmailCommand>
    {
        public RegistrationWithEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithDomainError(EmailErrors.EmptyEmail<RegistrationWithEmailCommandValidator>());

            RuleFor(x => x.Password).PasswordRules();

            RuleFor(x => x.ConfirmPassword).ConfirmPasswordRules(x => x.Password);

            RuleFor(x => x.AcceptTerms).AcceptTermsRules();
        }
    }
}
