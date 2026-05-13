using Authorization.Application.Exceptions;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Errors;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using Authorization.ErrorHandling;
using Authorization.Infrastructure.Exceptions;
using Authorization.Response;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Authorization.Middleware
{
    public class AuthExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<AuthExceptionHandler> _logger;
        private readonly IErrorMessageProvider _errorMessageProvider;
        private readonly IErrorStatusCodeMapper _errorStatusCodeMapper;

        public AuthExceptionHandler(
            ILogger<AuthExceptionHandler> logger,
            IErrorMessageProvider errorMessageProvider,
            IErrorStatusCodeMapper errorStatusCodeMapper)
        {
            _logger = logger;
            _errorMessageProvider = errorMessageProvider;
            _errorStatusCodeMapper = errorStatusCodeMapper;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var _userContext = httpContext.RequestServices.GetRequiredService<IUserContext>();

            if (exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                var response = new ErrorResponse
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorCode = ["VALIDATION_ERROR"],
                    Messages = errors
                };

                await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
                return true;
            }

            var errorCode = ExtractErrorCode(exception);

            var statusCode = _errorStatusCodeMapper.GetStatusCode(errorCode);

            var language = _userContext.GetLanguage();

            var message = _errorMessageProvider.GetMessage(errorCode, language);

            if (statusCode >= 500)
                _logger.LogError(exception, "Критична помилка: {Message}", exception.Message);
            else
                _logger.LogWarning(exception, "Помилка додатка [{Code}]: {Message}", errorCode, exception.Message);

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
                BusinessRuleViolationDomainException ex => ex.ErrorCode,
                DataInconsistencyDomainException ex => ex.ErrorCode,
                ValidationDomainException ex => ex.ErrorCode,
                MissingRateLimitConfigurationException ex => ex.ErrorCode,
                RateLimitExceededException ex => ex.ErrorCode,
                AlreadyExistsException ex => ex.ErrorCode,
                _ => "INTERNAL_ERROR"
            };
        }
    }
}