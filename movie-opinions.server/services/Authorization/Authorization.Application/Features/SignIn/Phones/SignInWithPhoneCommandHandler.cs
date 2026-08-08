using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using MediatR;

namespace Authorization.Application.Features.SignIn.Phones
{
    public class SignInWithPhoneCommandHandler  : IRequestHandler<SignInWithPhoneCommand, Result<SignInResult<Guid>>>
    {
        private readonly SignInFlowCoordinator _signInFlowCoordinator;

        public SignInWithPhoneCommandHandler(SignInFlowCoordinator signInFlowCoordinator)
        {
            _signInFlowCoordinator = signInFlowCoordinator;
        }

        public async Task<Result<SignInResult<Guid>>> Handle(SignInWithPhoneCommand command, CancellationToken cancellationToken = default)
        {
            var conutryCodeResult = PhoneCountryCode.Create(command.CountryCode);

            if (conutryCodeResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(conutryCodeResult.Errors);

            var nationalNumberResult = PhoneNationalNumber.Create(command.PhoneNumber);

            if (nationalNumberResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(nationalNumberResult.Errors);

            var phoneResult = Phone.Create(conutryCodeResult.Value, nationalNumberResult.Value);

            if (!phoneResult.IsSuccess)
                return Result<SignInResult<Guid>>.Failure(phoneResult.Errors);

            var login = new PhoneLogin(phoneResult.Value);

            var flowResult = await _signInFlowCoordinator.ProcessAsync(login, command.Password, cancellationToken);

            if (flowResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(flowResult.Errors);

            return flowResult;
        }
    }
}
