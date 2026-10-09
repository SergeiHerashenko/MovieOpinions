using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users
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
        #region Fields
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; }

        public int FailedPasswordAttempts { get; private set; }

        private readonly List<UserRefreshToken> _refreshTokens;

        public IReadOnlyCollection<UserRefreshToken> RefreshTokens
            => _refreshTokens.AsReadOnly();

        private readonly List<UserRestrictionSession> _restrictionSessions;

        public IReadOnlyCollection<UserRestrictionSession> RestrictionSessions
            => _restrictionSessions.AsReadOnly();

        private readonly List<UserRestriction> _restrictions;

        public IReadOnlyCollection<UserRestriction> Restrictions
            => _restrictions.AsReadOnly();

        private UserDeletion? _deletion;

        public bool IsDeleted => _deletion is not null;

        private UserPendingAction? _action;

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
        /// <param name="login">Перевірений логін користувача.</param>
        /// <param name="password">Пароль користувача у вигляді хешу.</param>
        /// <param name="now">Час створення користувача.</param>
        /// <returns>Створений агрегат користувача.</returns>
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
