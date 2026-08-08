using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Exceptions;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common
{
    internal static class LoginMapper
    {
        public static Login Restore<TValue>(
            NpgsqlDataReader reader,
            int loginTypeOrd,
            int loginOrd,
            int countryCodeOrd,
            int emailDomainOrd,
            Guid entityId)
        {
            var loginType = EnumMapper.Restore<LoginType>(
                reader.GetString(loginTypeOrd),
                typeof(TValue).Name,
                entityId
            );

            return loginType switch
            {
                LoginType.Email => RestoreEmailLogin(reader, emailDomainOrd, reader.GetString(loginOrd), typeof(TValue).Name, entityId),
                LoginType.Phone => RestorePhoneLogin(reader, countryCodeOrd, reader.GetString(loginOrd), typeof(TValue).Name, entityId),
                _ => throw DataConsistencyException.UnknownType(
                        $"Unknown type for {nameof(LoginType)}",
                        new Dictionary<string, object>
                        {
                            ["LoginType"] = reader.GetString(loginTypeOrd),
                            ["Entity"] = typeof(TValue).Name,
                            ["Id"] = entityId
                        }
                    )
            };
        }

        private static Login RestorePhoneLogin(NpgsqlDataReader reader, int countryOrd, string fullNumber, string entityName, Guid entityId)
        {
            var countryCode = reader.IsDBNull(countryOrd) ? null : reader.GetString(countryOrd);

            if (countryCode is null)
                throw DataConsistencyException.InvalidData(
                    $"Invalid data for {nameof(countryCode)}",
                    new Dictionary<string, object>
                    {
                        ["ValueName"] = nameof(countryCode),
                        ["Entity"] = entityName,
                        ["FullNumber"] = fullNumber,
                        ["Id"] = entityId
                    }
                );

            var phoneNumber = fullNumber.StartsWith(countryCode)
                ? fullNumber.Substring(countryCode.Length)
                : fullNumber;

            return PhoneLogin.Restore(countryCode, phoneNumber);
        }

        private static Login RestoreEmailLogin(NpgsqlDataReader reader, int emailDomainOrd, string fullEmail, string entityName, Guid entityId)
        {
            var emailDomainPart = reader.IsDBNull(emailDomainOrd) ? null : reader.GetString(emailDomainOrd);

            if (emailDomainPart is null)
                throw DataConsistencyException.InvalidData(
                    $"Invalid data for {nameof(emailDomainPart)}",
                    new Dictionary<string, object>
                    {
                        ["ValueName"] = nameof(emailDomainPart),
                        ["Entity"] = entityName,
                        ["FullEmail"] = fullEmail,
                        ["Id"] = entityId
                    }
                );

            return EmailLogin.Restore(fullEmail);
        }
    }
}
