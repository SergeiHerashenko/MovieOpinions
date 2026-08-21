using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserRestrictionSession
{
    public sealed class DeletedUserRestrictionSessionHandler : IRequestHandler<AggregateChangeRequest<UserRestrictionSessionDeleted>>
    {
        private readonly IUserRestrictionSessionRepository _userRestrictionSessionRepository;

        public DeletedUserRestrictionSessionHandler(IUserRestrictionSessionRepository userRestrictionSessionRepository)
        {
            _userRestrictionSessionRepository = userRestrictionSessionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserRestrictionSessionDeleted> change,
            CancellationToken cancellationToken = default)
        {
            await _userRestrictionSessionRepository.DeleteRestrictionSessionAsync(change.AggregateChange.UserRestrictionSessionId, cancellationToken);
        }
    }
}
