using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserRestrictionSession
{
    public sealed class UpdateUserRestrictionSessionHandler : IRequestHandler<AggregateChangeRequest<UserRestrictionSessionUpdated>>
    {
        private readonly IUserRestrictionSessionRepository _userRestrictionSessionRepository;

        public UpdateUserRestrictionSessionHandler(IUserRestrictionSessionRepository userRestrictionSessionRepository)
        {
            _userRestrictionSessionRepository = userRestrictionSessionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserRestrictionSessionUpdated> change,
            CancellationToken cancellationToken = default)
        {
            await _userRestrictionSessionRepository.UpdateRestrictionSessionAsync(change.AggregateChange.UserRestrictionSession, cancellationToken);
        }
    }
}
