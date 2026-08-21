namespace Authorization.Application.Common.Enums
{
    public enum RateLimitAction
    {
        Registration,

        ConfirmRegistration,

        Login,

        TwoFactor,

        PasswordChange,

        LoginChange,

        ChangeDeletingUser,

        SendChangeDeletionConfirmation,

        ConfirmationDeletingUser,

        ConfirmToken
    }
}
