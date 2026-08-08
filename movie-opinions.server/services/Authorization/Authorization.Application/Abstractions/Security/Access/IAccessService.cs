using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Abstractions.Security.Access
{
    public interface IAccessService<TMarker>
    {
        Task<Result> CheckUserAccess(User user);
    }
}
