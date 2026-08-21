using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionRepository.Ado
{
    internal class AdoUserRestrictionQueryRepository : QueryRepositoryBase<AdoUserRestrictionQueryRepository>
    {
        public AdoUserRestrictionQueryRepository(
            ILogger<AdoUserRestrictionQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }

        internal async Task<IReadOnlyList<UserRestriction>> GetRestrictionsByIdsAsync(IReadOnlyCollection<UserRestrictionId> userRestrictionIds, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT
                                id, created_at, user_id, restriction_rule, restriction_type, reason, restricted_by, is_revoked, cancellation_date 
                            FROM 
                                User_Restriction 
                            WHERE 
                                id = ANY(@Ids);";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@Ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                {
                    Value = userRestrictionIds.Select(x => x.Value).ToArray()
                });

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                var restrictions = new List<UserRestriction>();

                var ordinals = new UserRestrictionOrdinals(reader);

                while (await reader.ReadAsync(cancellationToken))
                {
                    restrictions.Add(UserRestrictionMapper.Map(reader, ordinals));
                }

                return restrictions;
            }, cancellationToken);
        }
    }
}
