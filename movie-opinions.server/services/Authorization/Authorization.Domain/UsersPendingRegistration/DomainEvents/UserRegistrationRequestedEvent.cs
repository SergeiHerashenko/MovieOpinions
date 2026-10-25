using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.UsersPendingRegistration.DomainEvents
{
    /// <summary>
    /// Доменна подія, яка фіксує запит на реєстрацію користувача.
    ///
    /// (Domain event indicating that user registration was requested.)
    /// </summary>
    public sealed class UserRegistrationRequestedEvent : DomainEvent
    {
        /// <summary>
        /// Ідентифікатор незавершеної реєстрації.
        ///
        /// (Identifier of the pending registration.)
        /// </summary>
        public UserPendingRegistrationId PendingRegistrationId { get; }

        /// <summary>
        /// Логін, для якого запитано реєстрацію.
        ///
        /// (Login for which registration was requested.)
        /// </summary>
        public Login Login { get; }

        /// <summary>
        /// Ініціалізує подію запиту на реєстрацію користувача.
        ///
        /// (Initializes a user registration request event.)
        /// </summary>
        /// <param name="pendingRegistrationId">
        /// Ідентифікатор незавершеної реєстрації.
        ///
        /// (Identifier of the pending registration.)
        /// </param>
        /// <param name="login">
        /// Логін, для якого запитано реєстрацію.
        ///
        /// (Login for which registration was requested.)
        /// </param>
        /// <param name="occurredOn">
        /// Час настання події.
        ///
        /// (Time when the event occurred.)
        /// </param>
        internal UserRegistrationRequestedEvent(
            UserPendingRegistrationId pendingRegistrationId,
            Login login,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            PendingRegistrationId = pendingRegistrationId;
            Login = login;
        }
    }
}
