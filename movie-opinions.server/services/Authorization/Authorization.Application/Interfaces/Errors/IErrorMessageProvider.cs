namespace Authorization.Application.Interfaces.Errors
{
    public interface IErrorMessageProvider
    {
        string GetMessage(string errorCode, string culture);
    }
}
