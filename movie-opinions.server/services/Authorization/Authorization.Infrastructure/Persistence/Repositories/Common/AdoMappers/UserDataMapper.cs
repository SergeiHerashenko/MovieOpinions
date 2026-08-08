using Authorization.Domain.Users;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.PasswordUser;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Models;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers
{
    internal static class UserDataMapper
    {
        public static UserData Map(NpgsqlDataReader reader, UserOrdinals ordinals)
        {
            var id = UserId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var login = LoginMapper.Restore<User>(
                reader,
                ordinals.LoginType,
                ordinals.Login,
                ordinals.PhoneCountryCode,
                ordinals.EmailDomainPart,
                id.Value
            );

            var passwordHash = new PasswordHash(reader.GetString(ordinals.PasswordHash));
            var password = Password.Restore(passwordHash);

            var role = EnumMapper.Restore<Role>(
                reader.GetString(ordinals.Role),
                nameof(User),
                id.Value
            );

            var updatedAt = reader.IsDBNull(ordinals.UpdatedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.UpdatedAt);
            var lastLoginAt = reader.IsDBNull(ordinals.LastLoginAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.LastLoginAt);

            var isLoginConfirmed = reader.GetBoolean(ordinals.IsLoginConfirmed);
            var failedLoginAttempts = reader.GetInt32(ordinals.FailedLoginAttempts);

            return new UserData
            {
                Id = id,
                CreatedAt = createdAt,
                LoginType = login.Type,
                Login = login,
                Password = password,
                Role = role,
                UpdatedAt = updatedAt,
                LastLoginAt = lastLoginAt,
                IsLoginConfirmed = isLoginConfirmed,
                FailedLoginAttempts = failedLoginAttempts
            };
        }
    }
}
