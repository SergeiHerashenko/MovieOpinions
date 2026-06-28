using Authorization.Application.Common.Events;
using Authorization.Application.Interfaces.Events;
using Authorization.Domain.Common.Models;
using MediatR;

namespace Authorization.Infrastructure.Events
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;

        public DomainEventDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var eventType = domainEvent.GetType();

            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);

            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if(notification is INotification mediatrNotification)
            {
                await _mediator.Publish(mediatrNotification, cancellationToken);
            }
        }
    }
}
