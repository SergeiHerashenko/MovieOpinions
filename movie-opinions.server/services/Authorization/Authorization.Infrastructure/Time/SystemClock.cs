using Authorization.Application.Interfaces.Time;

namespace Authorization.Infrastructure.Time
{
    public class SystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
