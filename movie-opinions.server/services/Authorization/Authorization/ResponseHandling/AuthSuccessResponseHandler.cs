using Authorization.ResponseHandling.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.ResponseHandling
{
    public class AuthSuccessResponseHandler : IAuthSuccessResponseHandler
    {
        public Task<IActionResult> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> HandleAsync<T>(
            T value, 
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
