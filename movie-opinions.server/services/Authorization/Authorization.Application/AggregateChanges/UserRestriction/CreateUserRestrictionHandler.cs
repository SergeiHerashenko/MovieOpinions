using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserRestriction
{
    public sealed class CreateUserRestrictionHandler : IRequestHandler<AggregateChangeRequest<UserRestrictionCreated>>
    {
        private readonly IUserRestrictionRepository _userRestrictionRepository;

        public CreateUserRestrictionHandler(IUserRestrictionRepository userRestrictionRepository)
        {
            _userRestrictionRepository = userRestrictionRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserRestrictionCreated> change, 
            CancellationToken cancellationToken = default)
        {
            await _userRestrictionRepository.CreateRestrictionAsync(change.AggregateChange.Restriction, cancellationToken);
        }
    }
}
