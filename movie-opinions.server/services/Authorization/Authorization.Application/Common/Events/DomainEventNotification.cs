using Authorization.Domain.Common.Models;
using MediatR;

namespace Authorization.Application.Common.Events
{
    public class DomainEventNotification<TEvent> : INotification
        where TEvent : IDomainEvent
    {
        public TEvent DomainEvent { get; }

        public DomainEventNotification(TEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
