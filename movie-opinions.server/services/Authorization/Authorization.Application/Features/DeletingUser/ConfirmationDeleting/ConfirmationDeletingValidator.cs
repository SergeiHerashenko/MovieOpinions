using FluentValidation;

namespace Authorization.Application.Features.DeletingUser.ConfirmationDeleting
{
    public sealed class ConfirmationDeletingValidator : AbstractValidator<ConfirmationDeletingCommand>
    {
        public ConfirmationDeletingValidator()
        {
            RuleFor(x => x.ConfirmationToken)
                .NotEmpty().WithMessage("The confirmation token is required!");

            RuleFor(x => x.VerificationValue)
                .NotEmpty().WithMessage("Будь ласка, введіть код підтвердження або скористайтеся валідним посиланням!");
        }
    }
}
