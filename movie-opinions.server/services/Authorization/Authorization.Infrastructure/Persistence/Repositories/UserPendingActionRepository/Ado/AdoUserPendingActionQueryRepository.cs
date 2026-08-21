using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingActionRepository.Ado
{
    internal class AdoUserPendingActionQueryRepository : QueryRepositoryBase<AdoUserPendingActionQueryRepository>
    {
        public AdoUserPendingActionQueryRepository(
            ILogger<AdoUserPendingActionQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }
    }
}
