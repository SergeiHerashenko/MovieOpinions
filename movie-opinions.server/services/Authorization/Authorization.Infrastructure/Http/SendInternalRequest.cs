using Authorization.Infrastructure.Http.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Authorization.Infrastructure.Http
{
    public class SendInternalRequest : ISendInternalRequest
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SendInternalRequest> _logger;

        public SendInternalRequest(
            IHttpClientFactory httpClientFactory, 
            ILogger<SendInternalRequest> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HttpResult<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest)
        {
            try
            {
                // 1. Створюємо іменований клієнт
                var client = _httpClientFactory.CreateClient(internalRequest.ClientName);
                
                // 2. Додаємо кастомні заголовки, якщо вони є
                if (internalRequest.Headers != null)
                {
                    foreach(var header in internalRequest.Headers)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                
                // 3. Відправка запиту
                HttpResponseMessage response = internalRequest.Method.Method.ToUpper() switch
                {
                    "POST" => await client.PostAsJsonAsync(internalRequest.Endpoint, internalRequest.Body),
                    "GET" => await client.GetAsync(internalRequest.Endpoint),
                    "PUT" => await client.PutAsJsonAsync(internalRequest.Endpoint, internalRequest.Body),
                    "DELETE" => await client.DeleteAsync(internalRequest.Endpoint),
                    _ => throw new NotSupportedException($"The method {internalRequest.Method} is not supported.")
                };

                // 4. Обробка успішної відповіді
                if (response.IsSuccessStatusCode)
                {
                    var data = response.StatusCode == System.Net.HttpStatusCode.NoContent
                        ? default
                        : await response.Content.ReadFromJsonAsync<TResponse>();

                    return HttpResult<TResponse>.Success(data!, (int)response.StatusCode);
                }

                // 5. Обробка помилок від зовнішнього сервісу
                var rawError = await response.Content.ReadAsStringAsync();

                string errorMessage = !string.IsNullOrWhiteSpace(rawError)
                    ? (rawError.Length > 200 ? rawError[..200] : rawError)
                    : $"The external service returned an error with status {(int)response.StatusCode}";

                _logger.LogWarning("HTTP request to {Endpoint} failed. Status: {StatusCode}. Error: {Error}",
                    internalRequest.Endpoint, 
                    (int)response.StatusCode, 
                    errorMessage
                );

                return HttpResult<TResponse>.Failure(errorMessage, (int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical(ex, "Network error (HttpRequestException) when requesting {Endpoint}", internalRequest.Endpoint);
                return HttpResult<TResponse>.Failure($"The network is unavailable or the service is down.: {ex.Message}", 503);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Critical error in infrastructure client when requesting {Endpoint}", internalRequest.Endpoint);
                return HttpResult<TResponse>.Failure($"Internal client error: {ex.Message}", 500);
            }
        }
    }
}
