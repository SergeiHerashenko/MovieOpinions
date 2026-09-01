using Authorization.Domain.Users.ValueObjects.PasswordUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado.Ordinals;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado.Mappers
{
    internal static class UserPendingRegistrationMapper
    {
        public static UserPendingRegistration Map(NpgsqlDataReader reader, UserPendingRegistrationOrdinals ordinals)
        {
            var id = UserPendingRegistrationId.Restore(reader.GetGuid(ordinals.Id));

            var login = LoginMapper.Restore<UserPendingRegistration>(
                reader,
                ordinals.LoginType,
                ordinals.Login,
                ordinals.CountryCode,
                ordinals.EmailDomain,
                id.Value
            );

            var passwprdHash = new PasswordHash(reader.GetString(ordinals.PasswordHash));
            var password = Password.Restore(passwprdHash);

            var registrationFlowToken = RegistrationFlowToken.Parse(reader.GetString(ordinals.RegistrationToken));

            var expiresAt = reader.GetFieldValue<DateTimeOffset>(ordinals.ExpiresAt);
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            return UserPendingRegistration.Restore(id, login, password, registrationFlowToken, createdAt, expiresAt);
        }
    }
}
