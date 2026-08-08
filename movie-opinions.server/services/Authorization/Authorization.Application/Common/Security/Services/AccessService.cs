using Authorization.Application.Abstractions.Security.Access;
using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Common.Security.Services
{
    public class AccessService<TMarker> : IAccessService<TMarker>
    {
        private readonly IEnumerable<IAccessStep<TMarker>> _accessSteps;

        public AccessService(IEnumerable<IAccessStep<TMarker>> accessSteps)
        {
            _accessSteps = accessSteps.OrderBy(c => c.Priority);
        }

        public async Task<Result> CheckUserAccess(User user)
        {
            foreach (var step in _accessSteps)
            {
                var result = await step.ExecuteAsync(user);

                if (result.IsFailure)
                    return result;
            }

            return Result.Success();
        }
    }
}
