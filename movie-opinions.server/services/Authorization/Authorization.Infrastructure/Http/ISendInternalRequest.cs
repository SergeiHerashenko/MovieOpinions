using Authorization.Infrastructure.Http.Models;

namespace Authorization.Infrastructure.Http
{
    public interface ISendInternalRequest
    {
        Task<HttpResult<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest, CancellationToken cancellationToken = default);

        Task<HttpResult<TResponse>> SendAsync<TResponse>(InternalRequest internalRequest, CancellationToken cancellationToken = default);
    }
}
