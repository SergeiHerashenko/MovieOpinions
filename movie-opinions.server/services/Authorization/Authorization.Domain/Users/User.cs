using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
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
    public partial class User : AggregateRoot<UserId, Guid>
    {
        private const int MaxFailedAttempts = 3;

        #region Fields
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; }

        public bool IsLoginConfirmed { get; private set; }

        public int FailedPasswordAttempts { get; private set; }

        private readonly List<UserRefreshToken> _refreshTokens = new();

        public IReadOnlyCollection<UserRefreshToken> RefreshTokens
            => _refreshTokens.AsReadOnly();

        private readonly List<UserRestrictionSession> _restrictionSessions = new();

        public IReadOnlyCollection<UserRestrictionSession> RestrictionSessions
            => _restrictionSessions.AsReadOnly();

        private readonly List<UserRestriction> _restrictions = new();

        public IReadOnlyCollection<UserRestriction> Restrictions
            => _restrictions.AsReadOnly();

        private UserDeletion? _deletion;

        public bool IsDeleted => _deletion is not null;

        private UserPendingAction? _action;

        public bool IsAction => _action is not null;
        #endregion

        #region Creation
        private User(UserId userId, Login login, Password password)
            : base(userId)
        {
            Login = login;
            Password = password;
            Role = Role.User;
            UpdatedAt = null;
            LastLoginAt = null;
            IsLoginConfirmed = true;
            FailedPasswordAttempts = 0;
            _restrictionSessions = new();
            _restrictions = new();
            _refreshTokens = new();
            _deletion = null;
            _action = null;
        }

        public static Result<User> Create(Login login, Password password)
        {
            if (login is null)
                return Result<User>.Failure(LoginErrors.EmptyLogin<User>());

            if (password is null)
                return Result<User>.Failure(PasswordErrors.EmptyHashPassword<User>());

            var user = new User(UserId.Create(), login, password);

            user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Login, user.CreatedAt));

            return Result<User>.Success(user);
        }
        #endregion

        #region Restoration
        private User(
            UserId userId,
            DateTimeOffset createdAt,
            Login login,
            Password password,
            Role role,
            DateTimeOffset? updateAt,
            DateTimeOffset? lastLoginAt,
            bool isLoginConfirmed,
            int failedLoginAttempts,
            IEnumerable<UserRestriction> restrictions,
            IEnumerable<UserRestrictionSession> restrictionsSessions,
            IEnumerable<UserRefreshToken> refreshTokens,
            UserPendingAction? pendingAction,
            UserDeletion? deletion)
            : base(userId, createdAt)
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = updateAt;
            LastLoginAt = lastLoginAt;
            IsLoginConfirmed = isLoginConfirmed;
            FailedPasswordAttempts = failedLoginAttempts;
            _restrictionSessions.AddRange(restrictionsSessions);
            _restrictions.AddRange(restrictions);
            _refreshTokens.AddRange(refreshTokens);
            _deletion = deletion;
            _action = pendingAction;
        }

        public static User Restore(
            UserId userId,
            DateTimeOffset createdAt,
            Login login,
            Password password,
            Role role,
            DateTimeOffset? updateAt,
            DateTimeOffset? lastLoginAt,
            bool isLoginConfirmed,
            int failedLoginAttempts,
            IEnumerable<UserRestriction> restrictions,
            IEnumerable<UserRestrictionSession> restrictionsSessions,
            IEnumerable<UserRefreshToken> refreshTokens,
            UserPendingAction? pendingAction,
            UserDeletion? deletion)
        {
            DomainGuard.AgainstNull<User>(
                (userId, nameof(userId)),
                (login, nameof(login)),
                (password, nameof(password))
            );

            if (!Enum.IsDefined(typeof(Role), role))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<User>(nameof(Role), role.ToString());

            return new User(userId, createdAt, login, password, role, updateAt, lastLoginAt,
                isLoginConfirmed, failedLoginAttempts, restrictions, restrictionsSessions, refreshTokens, pendingAction, deletion);
        }
        #endregion

        #region Guard
        private Result ProvideAccess()
        {
            if (_deletion is not null)
                return Result.Failure(DeletionErrors.UserIsDeleted<User>());

            if(_restrictionSessions.Any(x => x.RestrictionType == RestrictionType.Ban))
                return Result.Failure(RestrictionErrors.UserIsBlocked<User>());

            return Result.Success();
        }
        #endregion
    }
}