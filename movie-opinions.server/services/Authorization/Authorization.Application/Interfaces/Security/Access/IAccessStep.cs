using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Interfaces.Security.Access
{
    public interface IAccessStep
    {
        int Priority { get; }

        Task<Result> ExecuteAsync(User user);
    }
}
