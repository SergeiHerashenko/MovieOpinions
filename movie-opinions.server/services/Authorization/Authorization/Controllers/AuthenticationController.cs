using Authorization.Application.Features.Authentication.Registration;
using Authorization.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registration([FromBody] RegistrationRequest registrationRequest, CancellationToken cancellationToken)
        {
            var command = new RegistrationCommand(
                registrationRequest.Login,
                registrationRequest.Password,
                registrationRequest.ConfirmPassword,
                registrationRequest.AcceptTerms
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }
    }
}
