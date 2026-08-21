using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Action;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserPendingAction
{
    public sealed class UpdateUserPendingActionHandler : IRequestHandler<AggregateChangeRequest<UserPendingActionUpdated>>
    {
        private readonly IUserPendingActionRepository _userPendingActionRepository;

        public UpdateUserPendingActionHandler(IUserPendingActionRepository userPendingActionRepository)
        {
            _userPendingActionRepository = userPendingActionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserPendingActionUpdated> change,
            CancellationToken cancellationToken = default)
        {
            await _userPendingActionRepository.UpdateActionUserAsync(change.AggregateChange.UserPendingAction, cancellationToken);
        }
    }
}
