using FluentValidation;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser
{
    public class StartDeletingUserCommandValidator : AbstractValidator<StartDeletingUserCommand>
    {
        public StartDeletingUserCommandValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("The password field is required!");
        }
    }
}
