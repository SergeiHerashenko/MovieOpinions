using FluentValidation;

namespace Authorization.Application.Features.ChangingPassword.ConfirmationChangePassword
{
    public sealed class ConfirmationChangePasswordValidator
        : AbstractValidator<ConfirmationChangePasswordCommand>
    {
        public ConfirmationChangePasswordValidator()
        {
            RuleFor(x => x.ConfirmationToken)
                .NotEmpty().WithMessage("The confirmation token is required!");

            RuleFor(x => x.VerificationValue)
                .NotEmpty().WithMessage("Будь ласка, введіть код підтвердження або скористайтеся валідним посиланням!");
        }
    }
}
