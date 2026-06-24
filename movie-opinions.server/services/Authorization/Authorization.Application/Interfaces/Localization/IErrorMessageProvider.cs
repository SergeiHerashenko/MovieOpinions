namespace Authorization.Application.Interfaces.Localization
{
    public interface IErrorMessageProvider
    {
        string GetErrorMessage(string errorCode, string culture);
    }
}
