namespace Authorization.Application.Interfaces.Security
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
