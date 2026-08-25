namespace Authorization.Application.DTOs.Communication.Notifications.Enums
{
    public enum NotificationType
    {
        // Тип сповіщення - реєстрація користувача.
        StartRegistration = 0,

        ConfirmRegistration = 1,

        // Тип сповіщення - користувач ініціював дію.
        PendingChangePassword = 2,

        PendingResetPassword = 3,

        PendingChangeLogin = 4,

        PendingDeletingUser = 5,

        // Тип сповіщення - користувач підтвердив дію.
        ActionChangePassword = 6,

        ActionResetPassword = 7,

        ActionChangeLogin = 8,

        ActionDeletionUser = 9,

        // Тип сповіщення - новий вхід користувача.
        NewLogin = 10,

        // Тип сповіщення - користувач отримав обмеження.
        CreateSessionRestriction = 11,

        AddRestrictionSession = 12,

        // Тип сповіщення - користувачу змінили/зняли обмеження.
        RemoveRestrictionSession = 13,

        RemoveSession = 14,
    }
}
