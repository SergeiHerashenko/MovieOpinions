using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingActionRepository.Ado
{
    internal class AdoUserPendingActionCommandRepository : CommandRepositoryBase<AdoUserPendingActionCommandRepository>
    {
        public AdoUserPendingActionCommandRepository(
            ILogger<AdoUserPendingActionCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreateActionUserAsync(UserPendingAction entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Pending_Action (id, user_id, confirmation_token, action_type, login_type, action_data, expires_at, confirmation_time, expired_at, cancelled_time, status, created_at) 
                            VALUES 
                                (@Id, @UserId, @ConfirmationToken, @ActionType, @LoginType, @ActionData, @ExpiresAt, @ConfirmatonTime, @ExpiredAt, @CancelledTime, @Status, @CreatedAt);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddPendingActionUserParameters(command, entity);    
                
                await command.ExecuteNonQueryAsync(ct);

            }, cancellationToken);
        }

        internal async Task UpdateActionUserAsync(UserPendingAction entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Pending_Action 
                            SET 
                                confirmation_time = @ConfirmatonTime,
                                expired_at = @ExpiredAt,
                                cancelled_time = @CancelledTime,
                                status = @Status 
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
                command.Parameters.Add(new NpgsqlParameter("@ConfirmatonTime", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.ConfirmationTime) });
                command.Parameters.Add(new NpgsqlParameter("@ExpiredAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.ExpiredAt) });
                command.Parameters.Add(new NpgsqlParameter("@CancelledTime", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.CancelledTime) });
                command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = entity.Status.ToString() });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddPendingActionUserParameters(NpgsqlCommand command, UserPendingAction entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = entity.UserId.Value });
            command.Parameters.Add(new NpgsqlParameter("@ConfirmationToken", NpgsqlDbType.Varchar) { Value = entity.ConfirmationToken.Value });
            command.Parameters.Add(new NpgsqlParameter("@ActionType", NpgsqlDbType.Varchar) { Value = entity.UserAction.ActionType.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@LoginType", NpgsqlDbType.Varchar) 
            { 
                Value = entity.UserAction switch
                {
                    ChangeLoginAction loginAction => loginAction.Value,
                    ChangePasswordAction passwordAction => passwordAction.Value,
                    _ => DBNull.Value
                }
            });
            command.Parameters.Add(new NpgsqlParameter("@ActionData", NpgsqlDbType.Jsonb)
            {
                Value = entity.UserAction switch
                {
                    ChangePasswordAction passwordAction => JsonSerializer.Serialize(new
                    {
                        passwordAction.NewPassword.Value
                    }),

                    DeleteUserAction deleteAction => JsonSerializer.Serialize(new
                    {
                        deleteAction.Reason
                    }),

                    ChangeLoginAction loginAction => loginAction.NewLogin switch
                    {
                        EmailLogin emailLogin => JsonSerializer.Serialize(emailLogin.Email),
                        PhoneLogin phoneLogin => JsonSerializer.Serialize(phoneLogin.Phone),
                        _ => throw DataConsistencyException.UnknownType($"Unsupported login type: {loginAction.NewLogin.GetType()}")
                    },

                    _ => throw DataConsistencyException.UnknownType($"Unsupported action type: {entity.UserAction.GetType()}")
                }
            });
            command.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });
            command.Parameters.Add(new NpgsqlParameter("@ConfirmatonTime", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.ConfirmationTime) });
            command.Parameters.Add(new NpgsqlParameter("@ExpiredAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.ExpiredAt) });
            command.Parameters.Add(new NpgsqlParameter("@CancelledTime", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.CancelledTime) });
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = entity.Status.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
        }
    }
}
