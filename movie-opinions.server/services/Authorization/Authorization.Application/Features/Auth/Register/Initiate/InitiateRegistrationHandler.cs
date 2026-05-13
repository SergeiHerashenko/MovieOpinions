using Authorization.Application.Common.Enum;
using Authorization.Application.Errors;
using Authorization.Application.Exceptions;
using Authorization.Application.Features.Auth.Register.Initiate.Result;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Auth.Register.Initiate
{
    public class InitiateRegistrationHandler : IRequestHandler<InitiateRegistrationCommand, InitiateRegistrationResult>
    {
        private readonly ILogger<InitiateRegistrationHandler> _logger;

        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;
        private readonly IHasher _hasher;

        private readonly IPendingRegistrationService _pendingRegistrationService;
        private readonly IRegistrationNextStepResolver _registrationNextStepResolver;

        private readonly IUserRepository _userRepository;

        public InitiateRegistrationHandler(
            ILogger<InitiateRegistrationHandler> logger,
            IRateLimiter rateLomiter,
            IUserContext userContext,
            IPendingRegistrationService pendingRegistrationService,
            IUserRepository userRepository,
            IHasher hasher,
            IRegistrationNextStepResolver registrationNextStepResolver)
        {
            _rateLimiter = rateLomiter;
            _logger = logger;
            _userContext = userContext;
            _pendingRegistrationService = pendingRegistrationService;
            _userRepository = userRepository;
            _hasher = hasher;
            _registrationNextStepResolver = registrationNextStepResolver;
        }

        public async Task<InitiateRegistrationResult> Handle(InitiateRegistrationCommand command, CancellationToken cancellationToken)
        {
            await _rateLimiter.EnsureAllowedAsync(RateLimitAction.Registration, _userContext.GetIpAddress(), command.Login, cancellationToken);

            var loginVO = Login.Create(command.Login);

            cancellationToken.ThrowIfCancellationRequested();

            var exists = await _userRepository.ExistsByLoginAsync(loginVO, cancellationToken);

            if (exists)
            {
                throw new AlreadyExistsException(
                    ApplicationErrorCodes.ErrorUser.UserAlreadyExists, 
                    "User already exists"
                );
            }

            var passwordVO = Password.Create(_hasher.Hash(command.Password));

            var registration = await _pendingRegistrationService.CreateOrRefreshAsync(loginVO, passwordVO, cancellationToken);

            var nextStep = _registrationNextStepResolver.Resolve(loginVO);

            return new InitiateRegistrationResult
            {
                Message = "",
                NestStep = nextStep
            };
        }
    }
}