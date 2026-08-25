using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Features.Services.UserActionConfirmation.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.SendDeletionConfirmation
{
    public class SendDeletionConfirmationHandler : IRequestHandler<SendDeletionConfirmationCommand, Result<SendDeletionConfirmationResult>>
    {
        private readonly ISendUserActionConfirmationService _sendUserActionConfirmationService;

        public SendDeletionConfirmationHandler(
            ISendUserActionConfirmationService sendUserActionConfirmationService)
        {
            _sendUserActionConfirmationService = sendUserActionConfirmationService;
        }

        public async Task<Result<SendDeletionConfirmationResult>> Handle(
            SendDeletionConfirmationCommand command, 
            CancellationToken cancellationToken = default)
        {
            var sendData = new SendActionConfirmationData(
                command.ContactId,
                command.ConfirmationToken,
                command.MaskedValue
            );

            var sendResult = await _sendUserActionConfirmationService.SendAsync<DeleteAccountAction>(
                sendData, 
                cancellationToken
            );

            if (sendResult.IsFailure)
                return Result<SendDeletionConfirmationResult>.Failure(sendResult.Errors);

            var result = new SendDeletionConfirmationResult(
                sendResult.Value.ConfirmationNextStep,
                sendResult.Value.Message
            );

            return Result<SendDeletionConfirmationResult>.Success(result);
        }
    }
}
