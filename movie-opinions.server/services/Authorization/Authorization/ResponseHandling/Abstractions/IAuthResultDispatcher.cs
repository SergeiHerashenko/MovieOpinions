using Authorization.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.ResponseHandling.Abstractions
{
    public interface IAuthResultDispatcher
    {
        Task<IActionResult> DispatchAsync<T>(
            Result<T> result,
            CancellationToken cancellationToken = default);

        Task<IActionResult> DispatchAsync(
            Result result,
            CancellationToken cancellationToken = default);
    }
}
