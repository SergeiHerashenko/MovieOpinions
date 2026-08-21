using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Abstractions.Communication
{
    public interface INotificationSender
    {
        Task<Result> SendCreateNotificationAsync<TId>(NotificationRequest<TId> notificationCommand, CancellationToken cancellationToken = default);

        Task<Result> SendCreateNotificationAsync<TId, TData>(NotificationRequest<TId, TData> notificationCommand, CancellationToken cancellationToken = default);
    }
}
