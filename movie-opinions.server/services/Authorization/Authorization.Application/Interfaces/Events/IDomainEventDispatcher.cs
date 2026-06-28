using Authorization.Domain.Common.Models;

namespace Authorization.Application.Interfaces.Events
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
    }
}
