using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Interfaces.Orchestrator;
using Authorization.Domain.Results;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration.Steps
{
    public class ProfileStep(IProfileSender profileSender) : IOrchestratorStep<ConfirmRegistrationContext>
    {
        public int Order => 1;

        public async Task<Result> ExecuteAsync(ConfirmRegistrationContext context)
        {
            return await profileSender.SendCreateProfileRequestAsync(ProfileRequest.Create(
                context.UserId,
                context.Login,
                context.Role)
            );
        }

        public async Task RollbackAsync(ConfirmRegistrationContext context)
        {
            await profileSender.SendDeleteProfileRequestAsync(ProfileRequest.Create(
                context.UserId,
                context.Login,
                context.Role)
            );
        }
    }
}
