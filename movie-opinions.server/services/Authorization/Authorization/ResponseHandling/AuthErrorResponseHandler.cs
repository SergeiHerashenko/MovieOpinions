using Authorization.Application.Abstractions.UserContext;
using Authorization.Domain.Common.Errors;
using Authorization.ErrorHandling;
using Authorization.MessageHandling;
using Authorization.Response;
using Authorization.ResponseHandling.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.ResponseHandling
{
    public class AuthErrorResponseHandler : IAuthErrorResponseHandler
    {
        private readonly IErrorStatusCodeMapper _errorStatusCodeMapper;
        private readonly IErrorMessageProvider _errorMessageProvider;
        private readonly IUserContext _userContext;

        public AuthErrorResponseHandler(
            IErrorStatusCodeMapper errorStatusCodeMapper,
            IErrorMessageProvider errorMessageProvider,
            IUserContext userContext)
        {
            _errorMessageProvider = errorMessageProvider;
            _errorStatusCodeMapper = errorStatusCodeMapper;
            _userContext = userContext;
        }

        public IActionResult HandleAsync(
            IReadOnlyCollection<Error> erros)
        {
            var culture = _userContext.GetLanguage();

            var statusCode = _errorStatusCodeMapper.GetStatusCode(
                erros.First().Code
            );

            var errorCodes = new List<string>(erros.Count);
            var messages = new List<string>(erros.Count);

            foreach (var error in erros)
            {
                errorCodes.Add(error.Code);

                messages.Add(
                    _errorMessageProvider.GetErrorMessage(
                        error.Code,
                        culture
                    )
                );
            }

            var errorResponce = new ErrorResponse()
            {
                IsSuccess = false,
                StatusCode = statusCode,
                ErrorCode = errorCodes,
                Messages = messages
            };

            return new ObjectResult(errorResponce)
            {
                StatusCode = statusCode
            };
        }
    }
}
