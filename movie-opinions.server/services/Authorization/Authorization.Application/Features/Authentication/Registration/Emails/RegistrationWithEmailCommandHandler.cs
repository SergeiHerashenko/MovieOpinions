using Authorization.Application.Features.Authentication.Registration.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.Emails
{
    public class RegistrationWithEmailCommandHandler : IRequestHandler<RegistrationWithEmailCommand, Result<RegistrationResult>>
    {
        private readonly RegistrationFlowCoordinator _registrationFlowCoordinator;

        public RegistrationWithEmailCommandHandler(
            RegistrationFlowCoordinator registrationFlowCoordinator)
        {
            _registrationFlowCoordinator = registrationFlowCoordinator;
        }

        public async Task<Result<RegistrationResult>> Handle(RegistrationWithEmailCommand command, CancellationToken cancellationToken)
        {
            var emailResult = Email.Create(command.Email);

            if (!emailResult.IsSuccess)
                return Result<RegistrationResult>.Failure(emailResult.Errors);

            var login = new EmailLogin(emailResult.Value);

            var flowResult = await _registrationFlowCoordinator.ProcessAsync(
                login,
                command.Password,
                cancellationToken
            );

            if (flowResult.IsFailure)
                return Result<RegistrationResult>.Failure(flowResult.Errors);

            return Result<RegistrationResult>.Success(
                RegistrationResult.Success(
                    RegistrationNextStep.EmailConfirmation,
                    flowResult.Value.RegistrationToken.Value,
                    "Лист підтвердження відправлено на вашу пошту. Будь ласка, перевірте скриньку (та папку 'Спам', якщо листа немає в 'Вхідних')."
                )
            );
        }
    }
}
