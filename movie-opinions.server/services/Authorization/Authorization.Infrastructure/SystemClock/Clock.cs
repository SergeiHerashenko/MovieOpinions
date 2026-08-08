using Authorization.Application.Abstractions.Clock;

namespace Authorization.Infrastructure.SystemClock
{
    public class Clock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
