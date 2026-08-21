using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserRestrictionSession
{
    public sealed class CreateUserRestrictionSessionHandler : IRequestHandler<AggregateChangeRequest<UserRestrictionSessionCreated>>
    {
        private readonly IUserRestrictionSessionRepository _userRestrictionSessionRepository;

        public CreateUserRestrictionSessionHandler(IUserRestrictionSessionRepository userRestrictionSessionRepository)
        {
            _userRestrictionSessionRepository = userRestrictionSessionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserRestrictionSessionCreated> change, 
            CancellationToken cancellationToken = default)
        {
            await _userRestrictionSessionRepository.CreateRestrictionSessionAsync(change.AggregateChange.UserRestrictionSession, cancellationToken);
        }
    }
}
