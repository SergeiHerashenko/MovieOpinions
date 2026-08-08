using Authorization.Infrastructure.Exceptions;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common
{
    internal static class EnumMapper
    {
        public static TEnum Restore<TEnum>(string value, string entity, object entityId)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(value, out var result))
                return result;

            throw DataConsistencyException.UnknownType(
                $"Unknown type for {typeof(TEnum).Name}",
                new Dictionary<string, object>
                {
                    ["Value"] = value,
                    ["Entity"] = entity,
                    ["Id"] = entityId
                }
            );
        }
    }
}
