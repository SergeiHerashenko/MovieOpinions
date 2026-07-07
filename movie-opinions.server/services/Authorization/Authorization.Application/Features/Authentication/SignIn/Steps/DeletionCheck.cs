using Authorization.Application.Interfaces.Security.Access;
using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Features.Authentication.SignIn.Steps
{
    public class DeletionCheck : IAccessStep
    {
        public int Priority => 2;

        public async Task<Result> ExecuteAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
