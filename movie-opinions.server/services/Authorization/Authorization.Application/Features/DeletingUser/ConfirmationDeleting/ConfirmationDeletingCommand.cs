using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.ConfirmationDeleting
{
    public sealed class ConfirmationDeletingCommand : IRequest<Result>
    {
        public string ConfirmationToken { get; set; }

        public string VerificationValue { get; set; }

        public ConfirmationDeletingCommand(string confirmationToken, string verificationValue)
        {
            ConfirmationToken = confirmationToken;
            VerificationValue = verificationValue;
        }
    }
}
