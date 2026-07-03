using Authorization.Application.Features.Authentication.Registration.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.Phones
{
    public class RegistrationWithPhoneCommandHandler : IRequestHandler<RegistrationWithPhoneCommand, Result<RegistrationResult>>
    {
        private readonly RegistrationFlowCoordinator _registrationFlowCoordinator;

        public RegistrationWithPhoneCommandHandler(
            RegistrationFlowCoordinator registrationFlowCoordinator)
        {
            _registrationFlowCoordinator = registrationFlowCoordinator;
        }

        public async Task<Result<RegistrationResult>> Handle(RegistrationWithPhoneCommand command, CancellationToken cancellationToken = default)
        {
            var countryCode = CountryCode.Create(command.CountryCode);

            if (!countryCode.IsSuccess)
                return Result<RegistrationResult>.Failure(countryCode.Errors);

            var phoneResult = Phone.Create(countryCode.Value, command.PhoneNumber);

            if (!phoneResult.IsSuccess)
                return Result<RegistrationResult>.Failure(phoneResult.Errors);

            var login = new PhoneLogin(phoneResult.Value);

            var flowResult = await _registrationFlowCoordinator.ProcessAsync(
                login,
                command.Password,
                cancellationToken
            );

            if (flowResult.IsFailure)
                return Result<RegistrationResult>.Failure(flowResult.Errors);

            return Result<RegistrationResult>.Success(
                RegistrationResult.Success(
                    RegistrationNextStep.SmsConfirmation,
                    flowResult.Value.RegistrationToken.Value,
                    "Код підтвердження відправлено у SMS. Якщо код не надійшов протягом хвилини, ви можете запросити його повторно."
                )
            );
        }
    }
}
