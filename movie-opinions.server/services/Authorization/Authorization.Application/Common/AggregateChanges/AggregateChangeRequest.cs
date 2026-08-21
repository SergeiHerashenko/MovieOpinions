using Authorization.Domain.Common.Models.Interfaces;
using MediatR;

namespace Authorization.Application.Common.AggregateChanges
{
    public sealed class AggregateChangeRequest<TChange> : IRequest
        where TChange : IAggregateChange
    {
        public TChange AggregateChange { get; }

        public AggregateChangeRequest(TChange aggregateChange)
        {
            AggregateChange = aggregateChange;
        }
    }
}
