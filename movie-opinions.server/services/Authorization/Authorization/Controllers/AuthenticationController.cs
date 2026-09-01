using Authorization.Application.Features.LogOut;
using Authorization.Application.Features.Registration.ConfirmRegistration;
using Authorization.Application.Features.Registration.StartRegistration.Emails;
using Authorization.Application.Features.Registration.StartRegistration.Phones;
using Authorization.Application.Features.SignIn.Emails;
using Authorization.Application.Features.SignIn.Phones;
using Authorization.Cookie;
using Authorization.Requests.ConfirmRegistration;
using Authorization.Requests.Login;
using Authorization.Requests.Registration;
using Authorization.ResponseHandling.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Authorization.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICookieProvider _cookieProvider;
        private readonly IAuthResultDispatcher _authResultDispatcher;

        public AuthenticationController(
            IMediator mediator,
            ICookieProvider cookieProvider,
            IAuthResultDispatcher authResultDispatcher)
        {
            _mediator = mediator;
            _cookieProvider = cookieProvider;
            _authResultDispatcher = authResultDispatcher;
        }

        [HttpPost("register/email")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> RegistrationWithEmail(
            [FromBody] RegistrationWithEmailRequest registrationRequest, 
            CancellationToken cancellationToken = default)
        {
            var command = new RegistrationWithEmailCommand(
                registrationRequest.Email,
                registrationRequest.Password,
                registrationRequest.ConfirmPassword,
                registrationRequest.AcceptTerms
            );

            var result = await _mediator.Send(command, cancellationToken);

            return await _authResultDispatcher.DispatchAsync(result, cancellationToken);
        }

        [HttpPost("register/phone")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> RegistrationWithPhone(
            [FromBody] RegistrationWithPhoneRequest registrationRequest, 
            CancellationToken cancellationToken = default)
        {
            var command = new RegistrationWithPhoneCommand(
                registrationRequest.CountryCode,
                registrationRequest.PhoneNumber,
                registrationRequest.Password,
                registrationRequest.ConfirmPassword,
                registrationRequest.AcceptTerms
            );

            var result = await _mediator.Send(command, cancellationToken);

            return await _authResultDispatcher.DispatchAsync(result, cancellationToken);
        }

        [HttpPost("register/confirm-registration")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> RegistrationConfirm(
            [FromBody] ConfirmRegistrationRequest confirmRegistrationRequest, 
            CancellationToken cancellationToken = default)
        {
            var command = new ConfirmRegistrationCommand(
                confirmRegistrationRequest.RegistrationToken,
                confirmRegistrationRequest.VerificationValue
            );
        
            var result = await _mediator.Send(command,cancellationToken);
        
            if (result.IsFailure || result.Value.AccessToken is null || result.Value.RefreshToken is null)
                return BadRequest(result.Errors);
        
            _cookieProvider.SetCookies(result.Value.AccessToken, result.Value.RefreshToken);
        
            return Ok(result.Value);
        }
        
        [HttpPost("login/email")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> LoginWithEmail(
            [FromBody] LoginWithEmailRequest loginWithEmailRequest,
            CancellationToken cancellationToken = default)
        {
            var command = new SignInWithEmailCommand(
                loginWithEmailRequest.Email,
                loginWithEmailRequest.Password
            );
        
            var result = await _mediator.Send(command, cancellationToken);
        
            if (result.IsFailure || result.Value.AccessToken is null || result.Value.RefreshToken is null)
                return BadRequest(result.Errors);
        
            _cookieProvider.SetCookies(result.Value.AccessToken, result.Value.RefreshToken);
        
            return Ok(result.Value);
        }

        [HttpPost("login/phone")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> LoginWithPhone(
            [FromBody] LoginWithPhoneRequest loginWithPhoneRequest,
            CancellationToken cancellationToken = default)
        {
            var command = new SignInWithPhoneCommand(
                loginWithPhoneRequest.CountryCode,
                loginWithPhoneRequest.PhoneNumber,
                loginWithPhoneRequest.Password
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure || result.Value.AccessToken is null || result.Value.RefreshToken is null)
                return BadRequest(result.Errors);

            _cookieProvider.SetCookies(result.Value.AccessToken, result.Value.RefreshToken);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("logout")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LogOutCommand(), cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Errors);

            _cookieProvider.ClearCookies();

            return NoContent();
        }
    }
}
