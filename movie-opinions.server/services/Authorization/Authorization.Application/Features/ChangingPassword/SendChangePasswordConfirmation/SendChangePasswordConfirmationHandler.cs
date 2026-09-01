using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Features.Services.UserActionConfirmation.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using MediatR;

namespace Authorization.Application.Features.ChangingPassword.SendChangePasswordConfirmation
{
    public class SendChangePasswordConfirmationHandler
        : IRequestHandler<SendChangePasswordConfirmationCommand, Result<SendChangePasswordConfirmationResult>>
    {
        private readonly ISendUserActionConfirmationService _sendUserActionConfirmationService;

        public SendChangePasswordConfirmationHandler(
            ISendUserActionConfirmationService sendUserActionConfirmationService)
        {
            _sendUserActionConfirmationService = sendUserActionConfirmationService;
        }

        public async Task<Result<SendChangePasswordConfirmationResult>> Handle(
            SendChangePasswordConfirmationCommand command, 
            CancellationToken cancellationToken = default)
        {
            var sendData = new SendActionConfirmationData(
                command.ContactId,
                command.ConfirmationToken,
                command.MaskedValue
            );

            var sendResult = await _sendUserActionConfirmationService.SendAsync<PasswordChangeAction>(
                sendData,
                cancellationToken
            );

            if (sendResult.IsFailure)
                return Result<SendChangePasswordConfirmationResult>.Failure(sendResult.Errors);

            var result = new SendChangePasswordConfirmationResult(
                sendResult.Value.ConfirmationNextStep,
                sendResult.Value.Message
            );

            return Result<SendChangePasswordConfirmationResult>.Success(result);
        }
    }
}
