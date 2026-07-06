using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Communication
{
    public interface IContactsSender
    {
        Task<Result> SendCreateContactRequestAsync<TId>(ContactsRequest<TId> contactsRequest);

        Task<Result> SendUpdateContactRequestAsync<TId>(ContactsRequest<TId> contactsRequest);

        Task<Result> SendDeleteContactRequestAsync<TId>(ContactsRequest<TId> contactsRequest);
    }
}
