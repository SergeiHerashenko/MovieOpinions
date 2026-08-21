using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Action;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserPendingAction
{
    public sealed class CreateUserPendingActionHandler : IRequestHandler<AggregateChangeRequest<UserPendingActionCreated>>
    {
        private readonly IUserPendingActionRepository _userPendingActionRepository; 

        public CreateUserPendingActionHandler(IUserPendingActionRepository userPendingActionRepository)
        {
            _userPendingActionRepository = userPendingActionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserPendingActionCreated> change,
            CancellationToken cancellationToken = default)
        {
            await _userPendingActionRepository.CreateActionUserAsync(change.AggregateChange.UserPendingAction, cancellationToken);
        }
    }
}
