using Authorization.Application.Interfaces.Localization;

namespace Authorization.Infrastructure.Error
{
    public class ErrorMessageProvider : IErrorMessageProvider
    {
        public string GetErrorMessage(string errorCode, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
