using Authorization.Application.Interfaces.Security;

namespace Authorization.Infrastructure.Security
{
    public class Clock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
