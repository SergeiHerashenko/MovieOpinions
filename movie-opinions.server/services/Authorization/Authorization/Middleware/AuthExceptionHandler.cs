using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Localization;
using Authorization.Domain.Common.Exceptions;
using Authorization.ErrorHandling;
using Authorization.Response;
using Microsoft.AspNetCore.Diagnostics;

namespace Authorization.Middleware
{
    public class AuthExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<AuthExceptionHandler> _logger;
        private readonly IErrorStatusCodeMapper _errorStatusCodeMapper;
        private readonly IErrorMessageProvider _errorMessageProvider;

        public AuthExceptionHandler(
            ILogger<AuthExceptionHandler> logger,
            IErrorStatusCodeMapper errorStatusCodeMapper,
            IErrorMessageProvider errorMessageProvider)
        {
            _logger = logger;
            _errorStatusCodeMapper = errorStatusCodeMapper;
            _errorMessageProvider = errorMessageProvider;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var _userContext = httpContext.RequestServices.GetRequiredService<IUserContext>();

            var errorCode = ExtractErrorCode(exception);
            var statusCode = _errorStatusCodeMapper.GetStatusCode(errorCode);

            var language = _userContext.GetLanguage();

            var message = _errorMessageProvider.GetErrorMessage(errorCode, language);

            _logger.LogCritical(exception, "Критична помилка: {Message}", exception.Message);

            httpContext.Response.StatusCode = statusCode;

            var errorResponse = new ErrorResponse()
            {
                IsSuccess = false,
                Messages = new List<string>
                {
                    message
                },
                StatusCode = statusCode,
                ErrorCode = [errorCode]
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }

        private static string ExtractErrorCode(Exception exception)
        {
            return exception switch
            {
                BaseException ex => ex.ErrorCode,
                _ => "INTERNAL_ERROR"
            };
        }
    }
}
