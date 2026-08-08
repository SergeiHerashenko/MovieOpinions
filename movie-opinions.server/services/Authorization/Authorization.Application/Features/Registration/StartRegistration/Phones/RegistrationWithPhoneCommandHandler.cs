using Authorization.Application.Features.Registration.StartRegistration.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using MediatR;

namespace Authorization.Application.Features.Registration.StartRegistration.Phones
{
    public class RegistrationWithPhoneCommandHandler : IRequestHandler<RegistrationWithPhoneCommand, Result<RegistrationResult>>
    {
        private readonly RegistrationFlowCoordinator _registrationFlowCoordinator;

        public RegistrationWithPhoneCommandHandler(RegistrationFlowCoordinator registrationFlowCoordinator)
        {
            _registrationFlowCoordinator = registrationFlowCoordinator;
        }

        public async Task<Result<RegistrationResult>> Handle(RegistrationWithPhoneCommand command, CancellationToken cancellationToken = default)
        {
            var conutryCodeResult = PhoneCountryCode.Create(command.CountryCode);

            if (conutryCodeResult.IsFailure)
                return Result<RegistrationResult>.Failure(conutryCodeResult.Errors);

            var nationalNumberResult = PhoneNationalNumber.Create(command.PhoneNumber);

            if (nationalNumberResult.IsFailure)
                return Result<RegistrationResult>.Failure(nationalNumberResult.Errors);

            var phoneResult = Phone.Create(conutryCodeResult.Value, nationalNumberResult.Value);

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
                RegistrationResult.Create(
                    RegistrationNextStep.SmsConfirmation,
                    flowResult.Value.RegistrationFlowToken.Value,
                    "A verification code has been sent via SMS. If you don't receive the code within a minute, you can request it again!"
                )
            );
        }
    }
}
