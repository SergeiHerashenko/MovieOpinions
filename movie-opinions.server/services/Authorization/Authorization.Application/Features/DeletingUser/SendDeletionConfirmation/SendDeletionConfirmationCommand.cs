using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.SendDeletionConfirmation
{
    public sealed class SendDeletionConfirmationCommand : IRequest<Result<SendDeletionConfirmationResult>>
    {
        public string ConfirmationToken { get; }

        public string ContactId { get; }

        public string MaskedValue { get; }

        public SendDeletionConfirmationCommand(string confirmationToken, string contactId, string maskedValue)
        {
            ConfirmationToken = confirmationToken;
            ContactId = contactId;
            MaskedValue = maskedValue;
        }
    }
}
