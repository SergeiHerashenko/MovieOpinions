using FluentValidation;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationCommandValidator : AbstractValidator<ConfirmRegistrationCommand>
    {
        public ConfirmRegistrationCommandValidator()
        {
            RuleFor(x => x.RegistrationToken)
                .NotEmpty().WithMessage("Registration token is required. This is a structural request error!");

            RuleFor(x => x.VerificationValue)
                .NotEmpty().WithMessage("Будь ласка, введіть код підтвердження або скористайтеся валідним посиланням!");
        }
    }
}
