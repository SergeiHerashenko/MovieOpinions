using Authorization.Application.Features.Services.UserActiveContacts.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Abstractions.Services.UserActiveContacts
{
    public interface IActiveContactsProvider
    {
        Task<Result<IReadOnlyCollection<ActiveContactChannel>>> GetActiveContactsAsync(
            UserId userId,
            CancellationToken cancellationToken = default);
    }
}
