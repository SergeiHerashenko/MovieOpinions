using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationCommand : IRequest<Result<ConfirmRegistrationResult>>
    {
        public string RegistrationToken { get; }

        public string VerificationValue { get; }

        public ConfirmRegistrationCommand(string registrationToken, string verificationValue)
        {
            RegistrationToken = registrationToken;
            VerificationValue = verificationValue;
        }
    }
}
