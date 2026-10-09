namespace Authorization.Domain.Users.Entities.UsersRestrictionSession.Enums
{
    internal enum RestrictionSessionRemovalEffect
    {
        RemainsActive,

        BecomesEmpty,

        BecomesExpired,

        AlreadyExpired
    }
}
