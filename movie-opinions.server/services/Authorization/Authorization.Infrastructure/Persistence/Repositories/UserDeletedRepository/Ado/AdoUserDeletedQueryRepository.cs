using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.UserDeletedRepository.Ado
{
    internal class AdoUserDeletedQueryRepository : QueryRepositoryBase<AdoUserDeletedQueryRepository>
    {
        public AdoUserDeletedQueryRepository(
            ILogger<AdoUserDeletedQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }


    }
}
