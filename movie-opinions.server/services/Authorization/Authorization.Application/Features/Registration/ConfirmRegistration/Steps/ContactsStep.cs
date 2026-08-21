using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Orchestrator;
using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Domain.Results;

namespace Authorization.Application.Features.Registration.ConfirmRegistration.Steps
{
    public class ContactsStep(IContactsSender contactsSender) : IOrchestratorStep<ConfirmRegistrationContext>
    {
        public int Order => 2;

        public async Task<Result> ExecuteAsync(ConfirmRegistrationContext context)
        {
            return await contactsSender.SendCreateContactRequestAsync(CreateContactsRequest.Create(
                context.UserId,
                context.Login,
                context.CommunicationChannel)
            );
        }

        public async Task RollbackAsync(ConfirmRegistrationContext context)
        {
            await contactsSender.SendDeleteContactRequestAsync(DeleteContactRequest.Create(
                context.UserId,
                context.Login,
                context.CommunicationChannel)
            );
        }
    }
}
