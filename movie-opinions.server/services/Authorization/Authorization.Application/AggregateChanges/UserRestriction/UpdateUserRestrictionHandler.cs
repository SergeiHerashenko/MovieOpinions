using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserRestriction
{
    public sealed class UpdateUserRestrictionHandler : IRequestHandler<AggregateChangeRequest<UserRestrictionUpdated>>
    {
        private readonly IUserRestrictionRepository _userRestrictionRepository;

        public UpdateUserRestrictionHandler(IUserRestrictionRepository userRestrictionRepository)
        {
            _userRestrictionRepository = userRestrictionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserRestrictionUpdated> change, 
            CancellationToken cancellationToken = default)
        {
            await _userRestrictionRepository.UpdateRestrictionAsync(change.AggregateChange.UserRestriction, cancellationToken);
        }
    }
}
