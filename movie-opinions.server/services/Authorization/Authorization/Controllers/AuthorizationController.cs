using Authorization.Application.Features.Auth.Register.Initiate;
using Authorization.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthorizationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("registration-start")]
        public async Task<IActionResult> RegisterStart([FromBody] StartRegistrationRequest startRegistrationRequest, CancellationToken cancellationToken)
        {
            var command = new InitiateRegistrationCommand(
                startRegistrationRequest.Login,
                startRegistrationRequest.Password,
                startRegistrationRequest.ConfirmPassword,
                startRegistrationRequest.AcceptTerms);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
