using Authorization.Application.Common.ApplicationErrors.Users;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.Access;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Authentication.SignIn.Steps
{
    public class DeletionCheck : IAccessStep
    {
        private readonly IUserDeletedRepository _userDeletedRepository;

        private readonly IClock _clock;
        private readonly ILogger<DeletionCheck> _logger;

        public DeletionCheck(
            IUserDeletedRepository userDeletedRepository,
            IClock clock,
            ILogger<DeletionCheck> logger)
        {
            _userDeletedRepository = userDeletedRepository;
            _clock = clock;
            _logger = logger;
        }

        public int Priority => 2;

        public async Task<Result> ExecuteAsync(User user)
        {
            if (!user.IsDeleted)
                return Result.Success();

            var deletion = await _userDeletedRepository.GetDeletionUserById(user.Id);

            if(deletion is null)
            {
                _logger.LogWarning(
                    "User {UserId} is marked as deleted, but restriction session was not found.",
                    user.Id.Value
                );

                user.Undelete(_clock.UtcNow, _clock.UtcNow);

                return Result.Success();
            }

            return Result.Failure(UserErrors.ActiveDeleted(deletion.CreatedAt));
        }
    }
}
