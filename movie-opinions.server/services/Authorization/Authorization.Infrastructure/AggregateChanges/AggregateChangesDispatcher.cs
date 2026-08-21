using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Common.Models.Interfaces;
using MediatR;

namespace Authorization.Infrastructure.AggregateChanges
{
    public class AggregateChangesDispatcher : IAggregateChangesDispatcher
    {
        private readonly IMediator _mediator;

        public AggregateChangesDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task DispatchAsync(IAggregateChange aggregateChange, CancellationToken cancellationToken = default)
        {
            var changeType = aggregateChange.GetType();

            var requestType = typeof(AggregateChangeRequest<>).MakeGenericType(changeType);

            var request = Activator.CreateInstance(requestType, aggregateChange);

            if (request is IRequest mediatrRequest)
            {
                // TODO - додати ексепшин помилки 
            }

            await _mediator.Send(request, cancellationToken);
        }

        public async Task DispatchAsync(IReadOnlyCollection<IAggregateChange> aggregateChanges, CancellationToken cancellationToken = default)
        {
            foreach (var aggregateChange in aggregateChanges)
            {
                await DispatchAsync(aggregateChange, cancellationToken);
            }
        }
    }
}
