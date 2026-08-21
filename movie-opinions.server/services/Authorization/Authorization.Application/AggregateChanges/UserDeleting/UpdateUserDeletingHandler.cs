using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserDeleting
{
    public sealed class UpdateUserDeletingHandler : IRequestHandler<AggregateChangeRequest<UserDeletionUpdated>>
    {
        private readonly IUserDeletedRepository _userDeletedRepository;

        public UpdateUserDeletingHandler(IUserDeletedRepository userDeletedRepository)
        {
            _userDeletedRepository = userDeletedRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserDeletionUpdated> change,
            CancellationToken cancellationToken = default)
        {
            await _userDeletedRepository.UpdateDeletedUserAsync(change.AggregateChange.UserDeletion, cancellationToken);
        }
    }
}
