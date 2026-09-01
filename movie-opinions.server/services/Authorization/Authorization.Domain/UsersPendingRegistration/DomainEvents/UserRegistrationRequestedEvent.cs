using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;
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
