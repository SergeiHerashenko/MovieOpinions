namespace Authorization.Infrastructure.Security.JWT.Interfaces
{
    public interface IServiceJwtProvider
    {
        string GenerateServiceToken(string serviceName, string[] permissions);
    }
}
