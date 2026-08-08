using Authorization.Application.Features.Registration.StartRegistration.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using MediatR;

namespace Authorization.Application.Features.Registration.StartRegistration.Emails
{
    public class RegistrationWithEmailCommandHandler : IRequestHandler<RegistrationWithEmailCommand, Result<RegistrationResult>>
    {
        private readonly RegistrationFlowCoordinator _registrationFlowCoordinator;

        public RegistrationWithEmailCommandHandler(RegistrationFlowCoordinator registrationFlowCoordinator)
        {
            _registrationFlowCoordinator = registrationFlowCoordinator;
        }

        public async Task<Result<RegistrationResult>> Handle(RegistrationWithEmailCommand command, CancellationToken cancellationToken = default)
        {
            var emailResult = Email.Create(command.Email);

            if (emailResult.IsFailure)
                return Result<RegistrationResult>.Failure(emailResult.Errors);

            var login = new EmailLogin(emailResult.Value);

            var flowResult = await _registrationFlowCoordinator.ProcessAsync(
                login,
                command.Password,
                cancellationToken
            );

            if(flowResult.IsFailure)
                return Result<RegistrationResult>.Failure(flowResult.Errors);

            return Result<RegistrationResult>.Success(
                RegistrationResult.Create(
                    RegistrationNextStep.EmailConfirmation,
                    flowResult.Value.RegistrationFlowToken.Value,
                    "A confirmation email has been sent to your email. Please check your inbox (and your Spam folder if the email is not in your Inbox)!"
                )
            );
        }
    }
}
