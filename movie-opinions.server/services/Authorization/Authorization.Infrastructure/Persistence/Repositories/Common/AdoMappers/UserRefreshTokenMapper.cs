using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Npgsql;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers
{
    internal static class UserRefreshTokenMapper
    {
        public static UserRefreshToken Map(NpgsqlDataReader reader, UserRefreshTokenOrdinals ordinals)
        {
            var id = UserRefreshTokenId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var userId = UserId.Restore(reader.GetGuid(ordinals.UserId));

            var refreshToken = RefreshToken.Restore(reader.GetString(ordinals.RefreshToken));

            var deviceInfoJson = reader.GetString(ordinals.DeviceInfo);
            var deviceInfo = RestoreDeviceInfo(deviceInfoJson);

            var ipAddress = IpAddress.Restore(reader.GetString(ordinals.IpAddress));

            var city = reader.IsDBNull(ordinals.City) ? null : reader.GetString(ordinals.City);

            var tokenStatus = EnumMapper.Restore<TokenStatus>(
                reader.GetString(ordinals.TokenStatus),
                nameof(UserRefreshToken),
                id.Value
            );

            var expiresAt = reader.GetFieldValue<DateTimeOffset>(ordinals.ExpiresAt);

            var consumedAt = reader.IsDBNull(ordinals.ConsumedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.ConsumedAt);
            var revokedAt = reader.IsDBNull(ordinals.RevokedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.RevokedAt);

            return UserRefreshToken.Restore(id, userId, refreshToken, deviceInfo, ipAddress, city, tokenStatus, expiresAt, consumedAt, createdAt, revokedAt);
        }

        private static DeviceInfo RestoreDeviceInfo(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<DeviceInfo>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "DeviceInfo json is null.",
                        new Dictionary<string, object>
                        {
                            ["Entity"] = nameof(DeviceInfo)
                        }
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "DeviceInfo json has invalid format.",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(DeviceInfo)
                    },
                    ex
                );
            }
        }
    }
}
