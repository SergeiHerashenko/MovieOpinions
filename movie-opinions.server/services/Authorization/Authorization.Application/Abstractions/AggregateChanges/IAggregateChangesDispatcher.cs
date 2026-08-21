using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Application.Abstractions.AggregateChanges
{
    public interface IAggregateChangesDispatcher
    {
        Task DispatchAsync(IAggregateChange aggregateChange, CancellationToken cancellationToken = default);

        Task DispatchAsync(IReadOnlyCollection<IAggregateChange> aggregateChanges, CancellationToken cancellationToken = default);
    }
}
