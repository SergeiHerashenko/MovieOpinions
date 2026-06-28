using Authorization.Application.Interfaces.Localization;

namespace Authorization.Infrastructure.Errors
{
    public class ErrorMessageProvider : IErrorMessageProvider
    {
        public string GetErrorMessage(string errorCode, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
