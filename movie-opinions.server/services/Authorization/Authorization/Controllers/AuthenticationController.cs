using Authorization.Application.Features.Authentication.Registration.Emails;
using Authorization.Application.Features.Authentication.Registration.Phones;
using Authorization.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Authorization.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register/email")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> RegistrationWithEmail([FromBody] RegistrationWithEmailRequest registrationRequest, CancellationToken cancellationToken = default)
        {
            var command = new RegistrationWithEmailCommand(
                registrationRequest.Email,
                registrationRequest.Password,
                registrationRequest.ConfirmPassword,
                registrationRequest.AcceptTerms
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpPost("register/phone")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> RegistrationWithPhone([FromBody] RegistrationWithPhoneRequest registrationRequest, CancellationToken cancellationToken = default)
        {
            var command = new RegistrationWithPhoneCommand(
                registrationRequest.CountryCode,
                registrationRequest.PhoneNumber,
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
