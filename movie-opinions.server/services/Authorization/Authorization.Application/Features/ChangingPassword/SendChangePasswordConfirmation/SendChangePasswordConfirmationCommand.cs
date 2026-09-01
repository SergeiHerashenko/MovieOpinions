using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.ChangingPassword.SendChangePasswordConfirmation
{
    public sealed class SendChangePasswordConfirmationCommand 
        : IRequest<Result<SendChangePasswordConfirmationResult>>
    {
        public string ConfirmationToken { get; }

        public string ContactId { get; }

        public string MaskedValue { get; }

        public SendChangePasswordConfirmationCommand(
            string confirmationToken, 
            string contactId, 
            string maskedValue)
        {
            ConfirmationToken = confirmationToken;
            ContactId = contactId;
            MaskedValue = maskedValue;
        }
    }
}
