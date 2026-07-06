using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Interfaces.Orchestrator;
using Authorization.Domain.Results;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration.Steps
{
    public class ContactsStep(IContactsSender contactsSender) : IOrchestratorStep<ConfirmRegistrationContext>
    {
        public int Order => 2;

        public async Task<Result> ExecuteAsync(ConfirmRegistrationContext context)
        {
            return await contactsSender.SendCreateContactRequestAsync(ContactsRequest.Create(
                context.UserId,
                context.Login)
            );
        }

        public async Task RollbackAsync(ConfirmRegistrationContext context)
        {
            await contactsSender.SendDeleteContactRequestAsync(ContactsRequest.Create(
                context.UserId,
                context.Login)
            );
        }
    }
}
