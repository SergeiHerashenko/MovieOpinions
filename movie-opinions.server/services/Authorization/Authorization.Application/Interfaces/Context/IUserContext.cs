namespace Authorization.Application.Interfaces.Context
{
    public interface IUserContext
    {
        string GetLanguage();

        string GetIpAddress();
    }
}
