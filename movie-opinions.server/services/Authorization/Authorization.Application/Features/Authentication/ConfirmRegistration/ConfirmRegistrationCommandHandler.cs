using Authorization.Application.Common.ApplicationErrors.Confirm;
using Authorization.Application.Common.Enums;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Domain.Results;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using MediatR;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationCommandHandler : IRequestHandler<ConfirmRegistrationCommand, Result<ConfirmRegistrationResult>>
    {
        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;
        private readonly IUserRepository _userRepository;

        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;

        public ConfirmRegistrationCommandHandler(
            IUserPendingRegistrationRepository userPendingRegistrationRepository,
            IUserRepository userRepository,
            IRateLimiter rateLimiter)
        {
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
            _userRepository = userRepository;
            _rateLimiter = rateLimiter;
        }

        public async Task<Result<ConfirmRegistrationResult>> Handle(ConfirmRegistrationCommand command, CancellationToken cancellationToken = default)
        {
            var resultLimiter = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.ConfirmRegistration,
                _userContext.GetIpAddress(),
                command.RegistrationToken,
                cancellationToken
            );

            if (resultLimiter.IsFailure)
                return Result<ConfirmRegistrationResult>.Failure(resultLimiter.Errors);

            var registrationToken = RegistrationToken.Restore(command.RegistrationToken);

            var pendingRegistration = await _userPendingRegistrationRepository.GetByTokenAsync(registrationToken, cancellationToken);

            if (pendingRegistration is null)
                return Result<ConfirmRegistrationResult>.Failure(ConfirmErrors.InvalidOrExpiredToken<ConfirmRegistrationCommand>());


        }
    }
}
