using Authorization.Application.Features.Authentication.Registration.Enums;
using Authorization.Application.Result;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.Emails
{
    public class RegistrationWithEmailCommandHandler : IRequestHandler<RegistrationWithEmailCommand, ApplicationResult<RegistrationResult>>
    {
        private readonly RegistrationFlowCoordinator _registrationFlowCoordinator;

        public RegistrationWithEmailCommandHandler(
            RegistrationFlowCoordinator registrationFlowCoordinator)
        {
            _registrationFlowCoordinator = registrationFlowCoordinator;
        }

        public async Task<ApplicationResult<RegistrationResult>> Handle(RegistrationWithEmailCommand command, CancellationToken cancellationToken)
        {
            var emailResult = Email.Create(command.Email);

            if (!emailResult.IsSuccess)
                return ApplicationResult<RegistrationResult>.Failure(emailResult.Error);

            var login = new EmailLogin(emailResult.Value);

            var flowResult = await _registrationFlowCoordinator.ProcessAsync(
                login,
                command.Password,
                cancellationToken
            );

            if (flowResult.IsFailure)
                return ApplicationResult<RegistrationResult>.Failure(flowResult.Errors);

            return ApplicationResult<RegistrationResult>.Success(
                RegistrationResult.Success(
                    RegistrationNextStep.EmailConfirmation,
                    // TODO заглушка повідомлення, перевірити потім 
                    "Для завершення реєстрації будь-ласка перевірте пошту"
                )
            );
        }
    }
}
