using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Communication
{
    public interface IProfileSender
    {
        Task<Result> SendCreateProfileRequestAsync<TId>(ProfileRequest<TId> profileCommand);

        Task<Result> SendUpdateProfileRequestAsync<TId>(ProfileRequest<TId> profileCommand);

        Task<Result> SendDeleteProfileRequestAsync<TId>(ProfileRequest<TId> profileCommand);
    }
}
