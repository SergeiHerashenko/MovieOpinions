using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Orchestrator;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Services;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Application.DTOs.Communication.Verification;
using Authorization.Application.DTOs.Communication.Verification.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using MediatR;

namespace Authorization.Application.Features.Registration.ConfirmRegistration
{
    public class ConfirmRegistrationCommandHandler : IRequestHandler<ConfirmRegistrationCommand, Result<ConfirmRegistrationResult<Guid>>>
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;

        private readonly IVerificationSender _verificationSender;

        private readonly IOrchestrator<ConfirmRegistrationContext> _orchestrator;

        private readonly ITokenService _tokenService;

        private readonly IUserRepository _userRepository;
        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;
        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ConfirmRegistrationCommandHandler(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IVerificationSender verificationSender,
            IOrchestrator<ConfirmRegistrationContext> orchestrator,
            ITokenService tokenService,
            IUserRepository userRepository,
            IUserPendingRegistrationRepository userPendingRegistrationRepository,
            IUserRefreshTokenRepository userRefreshTokenRepository,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _verificationSender = verificationSender;
            _orchestrator = orchestrator;
            _tokenService = tokenService;
            _userRepository = userRepository;
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
            _userRefreshTokenRepository = userRefreshTokenRepository;
            _unitOfWork = unitOfWork;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result<ConfirmRegistrationResult<Guid>>> Handle(ConfirmRegistrationCommand command, CancellationToken cancellationToken = default)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if(ipAddressResult.IsFailure)
                return Result<ConfirmRegistrationResult<Guid>>.Failure(ipAddressResult.Errors);

            var ipAddress = ipAddressResult.Value;

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.ConfirmRegistration,
                ipAddress,
                command.RegistrationToken,
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<ConfirmRegistrationResult<Guid>>.Failure(limiterResult.Errors);

            var registrationToken = RegistrationFlowToken.Restore(command.RegistrationToken);

            var pendingRegistration = await _userPendingRegistrationRepository.GetPendingUserByTokenAsync(registrationToken, cancellationToken);

            if (pendingRegistration is null)
                return Result<ConfirmRegistrationResult<Guid>>.Failure(ConfirmErrors.InvalidOrExpiredToken<ConfirmRegistrationCommand>());

            var verificationCommand = VerificationRequest.Create(
                pendingRegistration.Id,
                VerificationType.ConfirmRegistration,
                command.VerificationValue
            );

            //var verificationResult = await _verificationSender.VerifyCodeAsync(verificationCommand);
            //
            //if (verificationResult.IsFailure)
            //    return Result<ConfirmRegistrationResult<Guid>>.Failure(verificationResult.Errors);

            var newUserResult = User.Create(pendingRegistration.Login, pendingRegistration.Password);

            if (newUserResult.IsFailure)
                return Result<ConfirmRegistrationResult<Guid>>.Failure(newUserResult.Errors);

            var newUser = newUserResult.Value;

            var channel = newUser.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var context = ConfirmRegistrationContext.Create(
                newUser.Id,
                newUser.Login,
                newUser.Role,
                channel,
                NotificationType.ConfirmRegistration
            );

            //var result = await _orchestrator.RunIntegrationsAsync(context);
            //
            //if (result.IsFailure)
            //    return Result<ConfirmRegistrationResult<Guid>>.Failure(result.Errors);

            var userToken = _tokenService.CreateUserSession(newUser);

            if (userToken.IsFailure)
                return Result<ConfirmRegistrationResult<Guid>>.Failure(userToken.Errors);

            await _unitOfWork.ExecuteAsync(async ct =>
            {
                await _userRepository.CreateUserAsync(newUser, ct);

                await _userRefreshTokenRepository.CreateRefreshTokenAsync(userToken.Value.UserRefreshToken, ct);

                await _userPendingRegistrationRepository.DeletePendingUserAsync(pendingRegistration.Id, ct);
            });

            await _domainEventDispatcher.DispatchAsync(newUserResult.Value.DomainEvents, cancellationToken);

            newUserResult.Value.ClearDomainEvents();

            var resultData = ConfirmRegistrationResult.Success(
                newUser.Id,
                Role.User,
                userToken.Value.AccessToken,
                userToken.Value.UserRefreshToken.RefreshToken.Value,
                "Реєстрація успішна!"
            );

            return Result<ConfirmRegistrationResult<Guid>>.Success(resultData);
        }
    }
}
