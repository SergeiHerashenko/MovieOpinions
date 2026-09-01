using Microsoft.AspNetCore.Mvc;

namespace Authorization.ResponseHandling.Abstractions
{
    public interface IAuthSuccessResponseHandler
    {
        Task<IActionResult> HandleAsync(
            CancellationToken cancellationToken = default);

        Task<IActionResult> HandleAsync<T>(
            T value,
            CancellationToken cancellationToken = default);
    }
}
