using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Orchestrator;
using Authorization.Application.Common.Enums;
using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Features.Registration.ConfirmRegistration.Steps
{
    public class NotificationStep(INotificationSender notificationSender) : IOrchestratorStep<ConfirmRegistrationContext>
    {
        public int Order => int.MaxValue;

        public async Task<Result> ExecuteAsync(ConfirmRegistrationContext context)
        {
            return await notificationSender.SendCreateNotificationAsync(NotificationRequest.Create(
                context.UserId,
                context.Login,
                MessageActions.ConfirmRegistration)
            );
        }

        public Task RollbackAsync(ConfirmRegistrationContext context)
        {
            return Task.CompletedTask;
        }
    }
}
