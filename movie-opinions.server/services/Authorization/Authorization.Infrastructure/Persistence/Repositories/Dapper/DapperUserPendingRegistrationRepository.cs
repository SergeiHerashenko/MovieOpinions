using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects.Login;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserPendingRegistrationRepository : RepositoryBase, IUserPendingRegistrationRepository
    {
        public DapperUserPendingRegistrationRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<DapperUserPendingRegistrationRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public Task<UserPendingRegistration> CreateAsync(UserPendingRegistration entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingRegistration> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingRegistration?> GetByLoginAsync(Login login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingRegistration> UpdateAsync(UserPendingRegistration entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
