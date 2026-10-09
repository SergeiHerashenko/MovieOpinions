namespace Authorization.Domain.Users.Enums
{
    public enum TokenStatus
    {
        Active = 0,

        Expired = 1,

        Consumed = 2,

        Revoked = 3
    }
}
