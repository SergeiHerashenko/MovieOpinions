using Authorization.Application.Features.DeletingUser.ConfirmationDeleting;
using Authorization.Application.Features.DeletingUser.SendDeletionConfirmation;
using Authorization.Application.Features.DeletingUser.StartDeletingUser;
using Authorization.Cookie;
using Authorization.Requests.DeletionUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Authorization.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICookieProvider _cookieProvider;

        public AccountController(
            IMediator mediator,
            ICookieProvider cookieProvider)
        {
            _mediator = mediator;
            _cookieProvider = cookieProvider;
        }

        //[Authorize]
        [HttpPost("deletion-user/start-deletion-user")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> StartDeletion(
            [FromBody] StartDeletionRequest startDeletionRequest,
            CancellationToken cancellationToken = default)
        {
            var command = new StartDeletingUserCommand(
                startDeletionRequest.Password,
                startDeletionRequest.Reason
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        //[Authorize]
        [HttpPost("deletion-user/send-deletion-confirmation")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> SendDeletionConfirmation(
            [FromBody] SendDeletionConfirmationRequest sendDeletionConfirmationRequest,
            CancellationToken cancellationToken = default)
        {
            var command = new SendDeletionConfirmationCommand(
                sendDeletionConfirmationRequest.ConfirmationToken,
                sendDeletionConfirmationRequest.ContactId,
                sendDeletionConfirmationRequest.MaskedValue
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        //[Authorize]
        [HttpPost("deletion-user/confirmation-deletion")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> ConfirmationDeletion(
            [FromBody] ConfirmationDeletionRequest confirmationDeletionRequest,
            CancellationToken cancellationToken = default)
        {
            var command = new ConfirmationDeletingCommand(
                confirmationDeletionRequest.ConfirmationToken,
                confirmationDeletionRequest.VerificationValue
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            _cookieProvider.ClearCookies();

            return Ok();
        }
    }
}
