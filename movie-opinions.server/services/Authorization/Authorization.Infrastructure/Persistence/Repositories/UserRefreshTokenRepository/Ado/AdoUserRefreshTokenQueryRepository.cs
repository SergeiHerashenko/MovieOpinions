using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRefreshTokenRepository.Ado
{
    internal class AdoUserRefreshTokenQueryRepository : QueryRepositoryBase<AdoUserRefreshTokenQueryRepository>
    {
        public AdoUserRefreshTokenQueryRepository(
            ILogger<AdoUserRefreshTokenQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }


    }
}
