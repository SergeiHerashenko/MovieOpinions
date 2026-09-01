using Authorization.Domain.Results;
using Authorization.ResponseHandling.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.ResponseHandling
{
    public class AuthResultDispatcher 
        : IAuthResultDispatcher
    {
        private readonly IAuthErrorResponseHandler _authErrorResponseHandler;
        private readonly IAuthSuccessResponseHandler _authSuccessResponseHandler;

        public AuthResultDispatcher(
            IAuthErrorResponseHandler authErrorResponseHandler, 
            IAuthSuccessResponseHandler authSuccessResponseHandler)
        {
            _authErrorResponseHandler = authErrorResponseHandler;
            _authSuccessResponseHandler = authSuccessResponseHandler;
        }

        public async Task<IActionResult> DispatchAsync<T>(
            Result<T> result, 
            CancellationToken cancellationToken = default)
        {
            if (result.IsFailure)
                return _authErrorResponseHandler.HandleAsync(
                    result.Errors
                );

            return await _authSuccessResponseHandler.HandleAsync(
                result.Value,
                cancellationToken
            );
        }

        public async Task<IActionResult> DispatchAsync(
            Result result, 
            CancellationToken cancellationToken = default)
        {
            if (result.IsFailure)
                return _authErrorResponseHandler.HandleAsync(
                    result.Errors
                );

            return await _authSuccessResponseHandler.HandleAsync(
                cancellationToken
            );
        }
    }
}
