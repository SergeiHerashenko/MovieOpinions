using Authorization.Application.Interfaces.Security.Access;
using Authorization.Application.Interfaces.Security.Services;
using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Common.Security.Services
{
    public class AccessService : IAccessService
    {
        private readonly IEnumerable<IAccessStep> _accessSteps;

        public AccessService(IEnumerable<IAccessStep> accessSteps)
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
