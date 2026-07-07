using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using MediatR;

namespace Authorization.Application.Features.Authentication.SignIn.Emails
{
    public class SignInWithEmailCommandHandler : IRequestHandler<SignInWithEmailCommand, Result<SignInResult<Guid>>>
    {
        private readonly SignInFlowCoordinator _signInFlowCoordinator;

        public SignInWithEmailCommandHandler(
            SignInFlowCoordinator signInFlowCoordinator)
        {
            _signInFlowCoordinator = signInFlowCoordinator;
        }

        public async Task<Result<SignInResult<Guid>>> Handle(SignInWithEmailCommand command, CancellationToken cancellationToken)
        {
            var emailResult = Email.Create(command.Email);

            if (emailResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(emailResult.Errors);

            var login = new EmailLogin(emailResult.Value);

            var flowResult = await _signInFlowCoordinator.ProcessAsync(login, command.Password, cancellationToken);

        }
    }
}
