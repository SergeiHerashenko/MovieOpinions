using Authorization.Application.Features.Authentication.Registration.Enums;
using Authorization.Application.Result;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.Phones
{
    public class RegistrationWithPhoneCommandHandler : IRequestHandler<RegistrationWithPhoneCommand, ApplicationResult<RegistrationResult>>
    {
        private readonly RegistrationFlowCoordinator _registrationFlowCoordinator;

        public RegistrationWithPhoneCommandHandler(
            RegistrationFlowCoordinator registrationFlowCoordinator)
        {
            _registrationFlowCoordinator = registrationFlowCoordinator;
        }

        public async Task<ApplicationResult<RegistrationResult>> Handle(RegistrationWithPhoneCommand command, CancellationToken cancellationToken = default)
        {
            var countryCode = CountryCode.Create(command.CountryCode);

            if (!countryCode.IsSuccess)
                return ApplicationResult<RegistrationResult>.Failure(countryCode.Error);

            var phoneResult = Phone.Create(countryCode.Value, command.PhoneNumber);

            if (!phoneResult.IsSuccess)
                return ApplicationResult<RegistrationResult>.Failure(phoneResult.Error);

            var login = new PhoneLogin(phoneResult.Value);

            var flowResult = await _registrationFlowCoordinator.ProcessAsync(
                login,
                command.Password,
                cancellationToken
            );

            if (flowResult.IsFailure)
                return ApplicationResult<RegistrationResult>.Failure(flowResult.Errors);

            return ApplicationResult<RegistrationResult>.Success(
                RegistrationResult.Success(
                    RegistrationNextStep.SmsConfirmation,
                    // TODO: Подумати над фінальним текстом повідомлення
                    ""
                )
            );
        }
    }
}
