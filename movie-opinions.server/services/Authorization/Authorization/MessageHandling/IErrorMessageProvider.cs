namespace Authorization.MessageHandling
{
    public interface IErrorMessageProvider
    {
        string GetErrorMessage(string errorCode, string culture);
    }
}
