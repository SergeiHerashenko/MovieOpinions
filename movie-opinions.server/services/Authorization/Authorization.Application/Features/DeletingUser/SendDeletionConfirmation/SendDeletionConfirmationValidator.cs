using FluentValidation;

namespace Authorization.Application.Features.DeletingUser.SendDeletionConfirmation
{
    public sealed class SendDeletionConfirmationValidator 
        : AbstractValidator<SendDeletionConfirmationCommand>
    {
        public SendDeletionConfirmationValidator()
        {
            RuleFor(x => x.ConfirmationToken)
                .NotEmpty().WithMessage("The confirmation token is required!");

            RuleFor(x => x.ContactId)
                .NotEmpty().WithMessage("The contact identifier is required!");
        }
    }
}
