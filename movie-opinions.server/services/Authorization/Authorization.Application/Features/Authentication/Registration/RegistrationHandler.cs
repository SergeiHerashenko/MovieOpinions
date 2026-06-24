using Authorization.Application.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationHandler : IRequestHandler<RegistrationCommand, ApplicationResult<RegistrationResult>>
    {
        private readonly ILogger<RegistrationHandler> _logger;

        public RegistrationHandler(
            ILogger<RegistrationHandler> logger)
        {
            _logger = logger;
        }

        public async Task<ApplicationResult<RegistrationResult>> Handle(RegistrationCommand command, CancellationToken cancellationToken)
        {
            return ApplicationResult<RegistrationResult>.Success(new RegistrationResult() { Message = "", NextStep = Enums.RegistrationNextStep.EmailConfirmation });
        }
    }
}
