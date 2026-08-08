using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Abstractions.Security.Access
{
    public interface IAccessStep<TMarker>
    {
        int Priority { get; }

        Task<Result> ExecuteAsync(User user, CancellationToken cancellationToken = default);
    }
}
