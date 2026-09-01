using FluentValidation;

namespace Authorization.Application.Features.ChangingPassword.SendChangePasswordConfirmation
{
    public sealed class SendChangePasswordConfirmationValidator
        : AbstractValidator<SendChangePasswordConfirmationCommand>
    {
        public SendChangePasswordConfirmationValidator()
        {
            RuleFor(x => x.ConfirmationToken)
                .NotEmpty().WithMessage("The confirmation token is required!");

            RuleFor(x => x.ContactId)
                .NotEmpty().WithMessage("The contact identifier is required!");
        }
    }
}
