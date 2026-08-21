using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Application.DTOs.Communication.Contacts.Responses;
using Authorization.Domain.Results;

namespace Authorization.Application.Abstractions.Communication
{
    public interface IContactsSender
    {
        Task<Result> SendCreateContactRequestAsync<TId>(CreateContactRequest<TId> request, CancellationToken cancellationToken = default);

        Task<Result> SendUpdateContactRequestAsync<TId>(UpdateContactRequest<TId> request, CancellationToken cancellationToken = default);

        Task<Result> SendDeleteContactRequestAsync<TId>(DeleteContactRequest<TId> request, CancellationToken cancellationToken = default);

        Task<Result<GetActiveChannelsResponse>> GetActiveContactsAsync<TId>(ActiveChannelsRequest<TId> request, CancellationToken cancellationToken = default);

        Task<Result<GetActiveContactResponse>> GetActiveContactAsync<TId>(ActiveContactRequest<TId> request, CancellationToken cancellationToken = default);
    }
}
