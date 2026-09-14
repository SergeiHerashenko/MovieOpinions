using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.LogOut
{
    public class LogOutCommandHandler : IRequestHandler<LogOutCommand, Result>
    {
        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<LogOutCommandHandler> _logger;
        private readonly IClock _clock;
        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;

        public LogOutCommandHandler(
            IUserRefreshTokenRepository userRefreshTokenRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<LogOutCommandHandler> logger,
            IClock clock,
            IAggregateChangesDispatcher aggregateChangesDispatcher)
        {
            _userRefreshTokenRepository = userRefreshTokenRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
            _clock = clock;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
        }

        public async Task<Result> Handle(LogOutCommand command, CancellationToken cancellationToken = default)
        {
            var userId = UserId.Restore(_userContext.GetUserId());

            var refreshTokenResult = RefreshToken.Restore(_userContext.GetRefreshToken());

            var result = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var userResult = await _userRepository.GetUserByIdForUpdateAsync(userId, ct);

                if(userResult is null)
                {
                    _logger.LogCritical(
                        "Logout could not inspect the refresh-token session because authenticated user {UserId} was not found.",
                        userId.Value
                    );

                    return Result.Success();
                }

                var refreshToken = await _userRefreshTokenRepository.GetByTokenForUpdateAsync(refreshTokenResult, ct);

                if (refreshToken is null)
                {
                    _logger.LogError("Logout completed without token revocation because the refresh token was not found!");

                    return Result.Success();
                }

                var revokeResult = userResult.RevokeRefreshToken(refreshToken.Id, _clock.UtcNow);

                if (revokeResult.IsFailure)
                {
                    _logger.LogCritical("Refresh token {RefreshTokenId} could not be revoked during logout. Error: {Error}.",
                        refreshToken.Id.Value,
                        revokeResult.Errors
                    );

                    return Result.Success();
                }

                await _aggregateChangesDispatcher.DispatchAsync(userResult.AggregateChanges, ct);

                return Result.Success();

            }, cancellationToken);

            return result;
        }
    }
}
