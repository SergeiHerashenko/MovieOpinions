namespace Authorization.Domain.Common.Validation.Enums
{
    public enum ValidationPriority
    {
        Presence = 10,

        BusinessRule = 20,

        Format = 30,

        Length = 40,
    }
}
