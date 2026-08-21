using Authorization.Domain.Users.Entities.UsersDeletion;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserDeletedRepository
    {
        Task CreateDeletedUserAsync(UserDeletion entity, CancellationToken cancellationToken = default);

        Task UpdateDeletedUserAsync(UserDeletion entity, CancellationToken cancellationToken= default);
    }
}
