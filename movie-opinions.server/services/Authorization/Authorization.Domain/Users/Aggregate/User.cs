using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Aggregate
{
    /// <summary>
    /// Корінь агрегату користувача. Керує його станом і змінами
    /// пов’язаних сутностей.
    ///
    /// Під час відновлення колекції дочірніх сутностей передаються
    /// з активними записами.
    ///
    /// (User aggregate root. Manages user state and changes
    /// to its related entities.
    ///
    /// During restoration, child-entity collections are supplied
    /// with active records.)
    /// </summary>
    public partial class User : AggregateRoot<UserId, Guid>
    {
        #region Properties
        /// <summary>
        /// Поточний логін користувача.
        ///
        /// (Current user login.)
        /// </summary>
        public Login Login { get; private set; }

        /// <summary>
        /// Доменне представлення поточного пароля користувача,
        /// що містить його хеш.
        ///
        /// (Domain representation of the user's current password,
        /// containing its hash.)
        /// </summary>
        public Password Password { get; private set; }

        /// <summary>
        /// Поточна роль користувача.
        ///
        /// (Current user role.)
        /// </summary>
        public Role Role { get; private set; }

        /// <summary>
        /// Час останнього зафіксованого оновлення стану користувача
        /// або null, якщо оновлення ще не зафіксоване.
        ///
        /// (Time of the last recorded user state update,
        /// or null if no update has been recorded.)
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; private set; }

        /// <summary>
        /// Час останнього успішного входу користувача
        /// або null, якщо успішного входу ще не було.
        ///
        /// (Time of the user's last successful sign-in,
        /// or null if the user has not signed in successfully.)
        /// </summary>
        public DateTimeOffset? LastLoginAt { get; private set; }

        /// <summary>
        /// Лічильник невдалих спроб перевірки пароля користувача.
        ///
        /// (Counter of failed attempts to verify the user's password.)
        /// </summary>
        public int FailedPasswordAttempts { get; private set; }

        private readonly List<UserRefreshToken> _refreshTokens;

        /// <summary>
        /// Колекція refresh-токенів користувача, якими керує агрегат.
        ///
        /// (Collection of user refresh tokens managed by the aggregate.)
        /// </summary>
        public IReadOnlyCollection<UserRefreshToken> RefreshTokens
            => _refreshTokens.AsReadOnly();

        private readonly List<UserRestrictionSession> _restrictionSessions;

        /// <summary>
        /// Колекція сесій обмежень користувача, якими керує агрегат.
        ///
        /// (Collection of user restriction sessions managed by the aggregate.)
        /// </summary>
        public IReadOnlyCollection<UserRestrictionSession> RestrictionSessions
            => _restrictionSessions.AsReadOnly();

        private readonly List<UserRestriction> _restrictions;

        /// <summary>
        /// Колекція обмежень користувача, якими керує агрегат.
        ///
        /// (Collection of user restrictions managed by the aggregate.)
        /// </summary>
        public IReadOnlyCollection<UserRestriction> Restrictions
            => _restrictions.AsReadOnly();

        private UserDeletion? _deletion;

        /// <summary>
        /// Вказує, чи агрегат містить поточний запис про видалення користувача.
        ///
        /// (Indicates whether the aggregate contains a current user deletion record.)
        /// </summary>
        public bool IsDeleted => _deletion is not null;

        private UserPendingAction? _action;

        /// <summary>
        /// Вказує, чи агрегат містить відкладену дію користувача.
        ///
        /// (Indicates whether the aggregate contains a pending user action.)
        /// </summary>
        public bool IsAction => _action is not null;
        #endregion

        #region Creation
        /// <summary>
        /// Установлює початковий стан нового користувача.
        ///
        /// (Initializes the state of a new user.)
        /// </summary>
        private User(
            UserId userId,
            Login login,
            Password password,
            DateTimeOffset now)
            : base(userId, now)
        {
            Login = login;
            Password = password;
            Role = Role.User;
            UpdatedAt = null;
            LastLoginAt = null;
            FailedPasswordAttempts = 0;
            _restrictionSessions = new();
            _restrictions = new();
            _refreshTokens = new();
            _deletion = null;
            _action = null;
        }

        /// <summary>
        /// Створює користувача з новим ідентифікатором і початковим станом
        /// та додає подію його реєстрації.
        ///
        /// (Creates a user with a new identifier and initial state,
        /// then records the registration event.)
        /// </summary>
        /// <param name="login">
        /// Перевірений логін користувача.
        /// 
        /// (Validated user login.)
        /// </param>
        /// <param name="password">
        /// Доменне представлення пароля користувача, що містить його хеш.
        /// 
        /// (Domain representation of the user's password, containing its hash.)
        /// </param>
        /// <param name="now">
        /// Час створення користувача.
        /// 
        /// (Time the user was created.)
        /// </param>
        /// <returns>
        /// Створений агрегат користувача.
        /// 
        /// (Created user aggregate.)
        /// </returns>
        public static User Create(
            Login login,
            Password password,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Create,
                (login, nameof(login)),
                (password, nameof(password))
            );

            var user = new User(
                UserId.Create(),
                login,
                password,
                now
            );

            user.AddDomainEvent(new UserRegisteredEvent(
                user.Id,
                user.Login,
                user.CreatedAt)
            );

            return user;
        }
        #endregion
    }
}
