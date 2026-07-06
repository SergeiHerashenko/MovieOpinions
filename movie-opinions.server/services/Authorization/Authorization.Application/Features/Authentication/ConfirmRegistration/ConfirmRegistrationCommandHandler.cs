using Authorization.Application.Common.ApplicationErrors.Confirm;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Security.Models;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Orchestrator;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using MediatR;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationCommandHandler : IRequestHandler<ConfirmRegistrationCommand, Result<ConfirmRegistrationResult>>
    {
        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;

        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;

        private readonly IVerificationSender _verificationSender;

        private readonly IOrchestrator<ConfirmRegistrationContext> _orchestrator;

        private readonly IUserRepository _userRepository;

        private readonly ITokenService _tokenService;

        public ConfirmRegistrationCommandHandler(
            IUserPendingRegistrationRepository userPendingRegistrationRepository,
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IVerificationSender verificationSender,
            IOrchestrator<ConfirmRegistrationContext> orchestrator,
            IUserRepository userRepository,
            ITokenService tokenService)
        {
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _verificationSender = verificationSender;
            _orchestrator = orchestrator;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<Result<ConfirmRegistrationResult>> Handle(ConfirmRegistrationCommand command, CancellationToken cancellationToken = default)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if(ipAddressResult.IsFailure)
                return Result<ConfirmRegistrationResult>.Failure(ipAddressResult.Errors);

            var ipAddress = ipAddressResult.Value;

            var resultLimiter = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.ConfirmRegistration,
                ipAddress,
                command.RegistrationToken,
                cancellationToken
            );

            if (resultLimiter.IsFailure)
                return Result<ConfirmRegistrationResult>.Failure(resultLimiter.Errors);

            var registrationToken = RegistrationToken.Restore(command.RegistrationToken);

            var pendingRegistration = await _userPendingRegistrationRepository.GetByTokenAsync(registrationToken, cancellationToken);

            if (pendingRegistration is null)
                return Result<ConfirmRegistrationResult>.Failure(ConfirmErrors.InvalidOrExpiredToken<ConfirmRegistrationCommand>());

            var verificationCommand = VerificationRequest.Create(
                pendingRegistration.UserId, 
                MessageActions.ConfirmRegistration, 
                command.VerificationValue
            );

            var verificationResult = await _verificationSender.VerifyCodeAsync(verificationCommand);
            
            if (verificationResult.IsFailure)
                return Result<ConfirmRegistrationResult>.Failure(verificationResult.Errors);

            var newUserResult = User.Create(pendingRegistration.Login, pendingRegistration.Password, Role.User, ipAddress);

            if (newUserResult.IsFailure)
                return Result<ConfirmRegistrationResult>.Failure(newUserResult.Errors);

            var creationResult = await _userRepository.CreateAsync(newUserResult.Value, cancellationToken);

            var context = ConfirmRegistrationContext.Create(
                creationResult.Id,
                creationResult.Login,
                creationResult.Role,
                MessageActions.ConfirmRegistration
            );

            var result = await _orchestrator.RunIntegrationsAsync(context);
            
            if (result.IsFailure)
            {
                await _userRepository.DeleteAsync(newUserResult.Value.Id);
            
                return Result<ConfirmRegistrationResult>.Failure(result.Errors);
            }

            var userSessionDTO = UserSessionDTO.Create(
                creationResult.Id,
                creationResult.Login,
                creationResult.Role,
                ipAddress
            );

            var userToken = await _tokenService.CreateUserSessionAsync(userSessionDTO);

            if (userToken.IsFailure)
                return Result<ConfirmRegistrationResult>.Failure(userToken.Errors);

            return Result<ConfirmRegistrationResult>.Success(ConfirmRegistrationResult.Success(
                Role.User,
                userToken.Value,
                "Реєстрація успішна!")
            );
        }
    }
}
