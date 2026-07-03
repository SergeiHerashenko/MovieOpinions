using Authorization.Infrastructure.Http.Models;

namespace Authorization.Infrastructure.Http
{
    public interface ISendInternalRequest
    {
        Task<HttpResult<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest);
    }
}
