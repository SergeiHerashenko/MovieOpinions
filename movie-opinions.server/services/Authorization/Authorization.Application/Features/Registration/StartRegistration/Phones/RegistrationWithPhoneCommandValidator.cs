using Authorization.Application.Common.Validation;
using Authorization.Domain.Common.Errors.Users;
using FluentValidation;

namespace Authorization.Application.Features.Registration.StartRegistration.Phones
{
    public class RegistrationWithPhoneCommandValidator : AbstractValidator<RegistrationWithPhoneCommand>
    {
        public RegistrationWithPhoneCommandValidator()
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty()
                    .WithDomainError(PhoneErrors.EmptyCountryCode<RegistrationWithPhoneCommandValidator>());

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                    .WithDomainError(PhoneErrors.EmptyNationalNumber<RegistrationWithPhoneCommandValidator>());

            RuleFor(x => x.Password).PasswordRules();

            RuleFor(x => x.ConfirmPassword).ConfirmPasswordRules(x => x.Password);

            RuleFor(x => x.AcceptTerms).AcceptTermsRules();
        }
    }
}
