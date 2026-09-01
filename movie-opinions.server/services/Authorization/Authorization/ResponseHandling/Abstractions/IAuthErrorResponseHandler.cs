using Authorization.Domain.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.ResponseHandling.Abstractions
{
    public interface IAuthErrorResponseHandler
    {
        IActionResult HandleAsync(IReadOnlyCollection<Error> errors);
    }
}
