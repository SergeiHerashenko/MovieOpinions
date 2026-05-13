namespace Authorization.Application.Interfaces.Time
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
