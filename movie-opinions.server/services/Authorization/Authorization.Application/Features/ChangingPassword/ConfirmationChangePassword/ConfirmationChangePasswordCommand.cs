using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.ChangingPassword.ConfirmationChangePassword
{
    public sealed class ConfirmationChangePasswordCommand
        : IRequest<Result>
    {
        public string ConfirmationToken { get; set; }

        public string VerificationValue { get; set; }

        public ConfirmationChangePasswordCommand(
            string confirmationToken,
            string verificationValue)
        {
            ConfirmationToken = confirmationToken;
            VerificationValue = verificationValue;
        }
    }
}
