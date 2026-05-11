using Authorization.Application.Common.Enum;
using Authorization.Application.Features.Auth.Register.Initiate.Result;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Security;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Auth.Register.Initiate
{
    public class InitiateRegistrationHandler : IRequestHandler<InitiateRegistrationCommand, InitiateRegistrationResult>
    {
        private readonly ILogger<InitiateRegistrationHandler> _logger;

        private readonly IRateLimiter _rateLomiter;
        private readonly IUserContext _userContext;

        public InitiateRegistrationHandler(ILogger<InitiateRegistrationHandler> logger,
            IRateLimiter rateLomiter,
            IUserContext userContext)
        {
            _rateLomiter = rateLomiter;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<InitiateRegistrationResult> Handle(InitiateRegistrationCommand request, CancellationToken cancellationToken)
        {
            await _rateLomiter.EnsureAllowedAsync(RateLimitAction.Register, _userContext.GetIpAddress(), request.Login, cancellationToken);

            return new InitiateRegistrationResult
            {
                Message = "",
                NestStep = Enum.InitiateRegistrationNextStep.EmailConfirmation
            };
        }
    }
}
