using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserDeleting
{
    public sealed class CreateUserDeletingHandler : IRequestHandler<AggregateChangeRequest<UserDeletionCreated>>
    {
        private readonly IUserDeletedRepository _userDeletedRepository;

        public CreateUserDeletingHandler(IUserDeletedRepository userDeletedRepository)
        {
            _userDeletedRepository = userDeletedRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserDeletionCreated> change,
            CancellationToken cancellationToken = default)
        {
            await _userDeletedRepository.CreateDeletedUserAsync(change.AggregateChange.UserDeletion, cancellationToken);
        }
    }
}
