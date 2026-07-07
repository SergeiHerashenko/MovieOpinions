using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Interfaces.Security.Services
{
    public interface IAccessService
    {
        Task<Result> CheckUserAccess(User user);
    }
}
