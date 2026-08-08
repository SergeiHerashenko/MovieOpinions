using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Change;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersPendingChange;
using Authorization.Domain.Users.Entities.UsersPendingChange.Changes;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Policies;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users
{
    public class User : AggregateRoot<UserId, Guid>
    {
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; }

        public bool IsLoginConfirmed { get; private set; }

        public int FailedLoginAttempts { get; private set; }

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

        private UserPendingChange? _change;

        public bool IsChange => _change is not null;

        private const int MaxFailedAttempts = 3;

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
            FailedLoginAttempts = 0;
            _restrictionSessions = new();
            _restrictions = new();
            _refreshTokens = new();
            _deletion = null;
            _change = null;
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
            UserPendingChange? pendingChange,
            UserDeletion? deletion)
            : base(userId, createdAt)
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = updateAt;
            LastLoginAt = lastLoginAt;
            IsLoginConfirmed = isLoginConfirmed;
            FailedLoginAttempts = failedLoginAttempts;
            _restrictionSessions.AddRange(restrictionsSessions);
            _restrictions.AddRange(restrictions);
            _refreshTokens.AddRange(refreshTokens);
            _deletion = deletion;
            _change = pendingChange;
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
            UserPendingChange? pendingChange,
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
                isLoginConfirmed, failedLoginAttempts, restrictions, restrictionsSessions, refreshTokens, pendingChange, deletion);
        }
        #endregion

        #region Behavior
        public Result RecordFailedLoginAttempt(DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            FailedLoginAttempts++;
            UpdatedAt = now;

            if(FailedLoginAttempts >= MaxFailedAttempts)
            {
                var restrictionData = CreateFailedLoginBanData();

                if (restrictionData.IsFailure)
                    return restrictionData;

                var addResult = AddRestrictions(new[] { restrictionData.Value }, now);

                if (addResult.IsFailure)
                    return addResult;

                FailedLoginAttempts = 0;
            }

            return Result.Success();
        }

        public Result LoginSuccess(DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            if (!IsLoginConfirmed)
                return Result.Failure(LoginErrors.LoginIsNotConfirm<User>());

            FailedLoginAttempts = 0;
            LastLoginAt = now;

            return Result.Success();
        }

        private Result<RestrictionData> CreateFailedLoginBanData()
        {
            var ruleResult = RestrictionRule.Create(RestrictionPolicy.FailedLoginBanName, RestrictionPolicy.FailedLoginBanDurationMinutes);

            if (ruleResult.IsFailure)
                return Result<RestrictionData>.Failure(ruleResult.Errors);

            return Result<RestrictionData>.Success(new RestrictionData
            {
                RestrictionType = RestrictionType.Ban,
                RestrictionRule = ruleResult.Value,
                Reason = RestrictionPolicy.FailedLoginBanReason,
                RestrictedBy = RestrictionPolicy.FailedLoginBanRestrictedBy
            });
        }
        #endregion

        #region Behavior (Tokens)
        public Result<UserRefreshToken> CreateRefreshToken(
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return Result<UserRefreshToken>.Failure(access.Errors);

            if (IsNewDevice(deviceInfo, ipAddress))
                AddDomainEvent(new UserSignedInFromNewDeviceEvent(Id, Login, deviceInfo, ipAddress, now));

            var tokenResult = UserRefreshToken.Create(Id, deviceInfo, ipAddress, now, city);

            if(tokenResult.IsFailure)
                return tokenResult;

            var refreshToken = tokenResult.Value;

            _refreshTokens.Add(refreshToken);

            AddAggregateChangeEvent(new UserRefreshTokenCreated(refreshToken, now));

            return Result<UserRefreshToken>.Success(refreshToken);
        }

        public Result ConsumeRefreshToken(UserRefreshTokenId userRefreshTokenId, DateTimeOffset now)
        {
            var refreshToken = _refreshTokens
                .FirstOrDefault(x => x.Id == userRefreshTokenId);

            if (refreshToken is null)
                return Result.Failure(RefreshTokenErrors.TokenStatus.NotFoundToken<User>());

            var consumeResult = refreshToken.Consume(now);

            if (consumeResult.IsFailure)
                return consumeResult;

            _refreshTokens.Remove(refreshToken);

            AddAggregateChangeEvent(new UserRefreshTokenUpdated(refreshToken, now));

            return Result.Success();
        }

        public Result RevokeRefreshToken(UserRefreshTokenId userRefreshTokenId, DateTimeOffset now)
        {
            var refreshToken = _refreshTokens
                .FirstOrDefault(x => x.Id == userRefreshTokenId);

            if (refreshToken is null)
                return Result.Failure(RefreshTokenErrors.TokenStatus.NotFoundToken<User>());

            var consumeResult = refreshToken.Revoke(now);

            if (consumeResult.IsFailure)
                return consumeResult;

            _refreshTokens.Remove(refreshToken);

            AddAggregateChangeEvent(new UserRefreshTokenUpdated(refreshToken, now));

            return Result.Success();
        }

        private bool IsNewDevice(DeviceInfo deviceInfo, IpAddress ipAddress)
        {
            return !_refreshTokens.Any(x => 
                x.DeviceInfo == deviceInfo && 
                x.IpAddress == ipAddress
            );
        }
        #endregion

        #region Behavior (Change)
        public Result ChangeLogin(Login newLogin, DateTimeOffset now)
        {
            if (newLogin is null)
                return Result.Failure(LoginErrors.EmptyLogin<User>());

            var access = ProvideAccess();

            if (access.IsFailure)
                return access;

            if (_change is not null)
                return Result.Failure(ChangeErrors.ChangeAlreadyExists<User>());

            if(newLogin == Login)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(Login)));

            var change = UserChange.From(newLogin);

            var createChangeResult = CreateChange(change, now);

            if(createChangeResult.IsFailure)
                return createChangeResult;

            AddAggregateChangeEvent(new UserPendingChangeCreated(createChangeResult.Value, now));

            return Result.Success();
        }

        public Result ChangePassword(Password newPassword, DateTimeOffset now)
        {
            if (newPassword is null)
                return Result.Failure(PasswordErrors.EmptyHashPassword<User>());

            var access = ProvideAccess();

            if (access.IsFailure)
                return access;

            if (_change is not null)
                return Result.Failure(ChangeErrors.ChangeAlreadyExists<User>());

            if (newPassword == Password)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(Password)));

            var change = UserChange.From(newPassword);

            var createChangeResult = CreateChange(change, now);

            if (createChangeResult.IsFailure)
                return createChangeResult;

            AddAggregateChangeEvent(new UserPendingChangeCreated(createChangeResult.Value, now));

            return Result.Success();
        }

        public Result<UserPendingChange> GetPendingChange()
        {
            if (_change is null)
                return Result<UserPendingChange>.Failure(ChangeErrors.EmptyChangeUser<User>());

            return Result<UserPendingChange>.Success(_change);
        }

        private Result<UserPendingChange> CreateChange(UserChange userChange, DateTimeOffset now)
        {
            var pendingResult = UserPendingChange.Create(Id, userChange, now);

            if (pendingResult.IsFailure)
                return pendingResult;

            _change = pendingResult.Value;

            return Result<UserPendingChange>.Success(pendingResult.Value);
        }
        #endregion

        #region Behavior (Confirm)
        public Result ConfirmLogin(string confirmationToken, DateTimeOffset now)
        {
            return CompleteChange<LoginChange>(
                confirmationToken,
                now,
                change =>
                {
                    Login = change.NewLogin;
                    IsLoginConfirmed = true;
                }
            );
        }

        public Result ConfirmPassword(string confirmationToken, DateTimeOffset now)
        {
            return CompleteChange<PasswordChange>(
                confirmationToken,
                now,
                change =>
                {
                    Password = change.NewPassword;
                }
            );
        }

        private Result CompleteChange<TChange>(
            string confirmationToken,
            DateTimeOffset now,
            Action<TChange> applyChange)
            where TChange : UserChange
        {
            var access = ProvideAccess();

            if (access.IsFailure)
                return access;

            if (_change is null)
                return Result.Failure(ChangeErrors.EmptyChangeUser<User>());

            var confirmResult = _change.ConfirmChange(confirmationToken, now);

            if (confirmResult.IsFailure)
                return confirmResult;

            var change = _change.UserChange as TChange;

            if (change is null)
                return Result.Failure(ChangeErrors.InvalidChangeType<User>());

            applyChange(change);

            UpdatedAt = now;

            var pendingChange = _change;

            _change = null;

            AddAggregateChangeEvent(new UserPendingChangeUpdated(pendingChange, now));

            return Result.Success();
        }
        #endregion

        #region Behavior (Removal)
        public Result Delete(DateTimeOffset now, string? reason = null)
        {
            if (IsDeleted)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var createDeletionResult = UserDeletion.Create(Id, Login, now, reason);

            if (createDeletionResult.IsFailure)
                return createDeletionResult;

            var deletion = createDeletionResult.Value;

            _deletion = deletion;

            AddAggregateChangeEvent(new UserDeletionCreated(deletion, now));

            return Result.Success();
        }

        public Result Undelete(DateTimeOffset now)
        {
            if(_deletion is null)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var result = _deletion.Undelete(now);

            if(result.IsFailure)
                return result;

            AddAggregateChangeEvent(new UserDeletionUpdated(_deletion, now));

            return Result.Success();
        }

        public Result<UserDeletion> GetDeletion()
        {
            if (_deletion is null)
                return Result<UserDeletion>.Failure(DeletionErrors.NotDeleteUser<User>());

            return Result<UserDeletion>.Success(_deletion);
        }

        public bool UpdateExpirationStatus(DateTimeOffset now)
        {
            if (_deletion is null)
                return false;

            if (!_deletion.MarkAsExpired(now))
                return false;

            AddAggregateChangeEvent(new UserDeletionUpdated(_deletion, now));

            return true;
        }
        #endregion

        #region Behavior (Restriction)
        public Result AddRestrictions(IEnumerable<RestrictionData> restrictionDataCollection, DateTimeOffset now)
        {
            if (restrictionDataCollection is null)
                return Result.Failure(RestrictionErrors.EmptyRestrictionList<User>());

            var restrictionData = restrictionDataCollection.ToList();

            if (restrictionData.Count == 0)
                return Result.Failure(RestrictionErrors.EmptyRestrictionList<User>());

            var createRestrictionsResult = CreateRestrictions(restrictionData);

            if (createRestrictionsResult.IsFailure)
                return createRestrictionsResult;

            foreach (var restrictionGroup in createRestrictionsResult.Value.GroupBy(x => x.RestrictionType))
            {
                var session = _restrictionSessions
                    .FirstOrDefault(x => x.RestrictionType == restrictionGroup.Key);

                if (session is null)
                {
                    var createSessionResult = UserRestrictionSession.Create(Id, restrictionGroup);

                    if (createSessionResult.IsFailure)
                        return createSessionResult;

                    _restrictionSessions.Add(createSessionResult.Value);

                    AddAggregateChangeEvent(new UserRestrictionSessionCreated(createSessionResult.Value, now));

                    continue;
                }

                var addRestrictionsResult = session.AddRestrictions(restrictionGroup);

                if (addRestrictionsResult.IsFailure)
                    return addRestrictionsResult;

                AddAggregateChangeEvent(new UserRestrictionSessionUpdated(session, now));
            }

            _restrictions.AddRange(createRestrictionsResult.Value);

            AddRestrictionCreatedEvents(createRestrictionsResult.Value, now);

            return Result.Success();
        }

        public Result RemoveRestriction(UserRestrictionId userRestrictionId, DateTimeOffset now)
        {
            if (userRestrictionId is null)
                return Result.Failure(CommonErrors.Identifier.EmptyIdentifier<UserRestrictionId>(nameof(userRestrictionId)));

            var restriction = _restrictions
                .FirstOrDefault(x => x.Id == userRestrictionId);

            if (restriction is null)
                return Result.Failure(RestrictionErrors.NotFoundRestriction<User>());

            var resultCancel = restriction.CancelRestriction(now);

            if (resultCancel.IsFailure)
                return resultCancel;

            var session = _restrictionSessions
                .FirstOrDefault(x => x.RestrictionType == restriction.RestrictionType);

            if (session is null)
                return Result.Failure(RestrictionErrors.NotFoundSession<User>(restriction.RestrictionType.ToString()));

            var removeResult = session.RemoveRestriction(restriction);

            if (removeResult.IsFailure)
                return removeResult;

            if (session.IsEmpty())
            {
                _restrictionSessions.Remove(session);

                AddAggregateChangeEvent(new UserRestrictionSessionDeleted(session.Id, now));
            }
            else
            {
                AddAggregateChangeEvent(new UserRestrictionSessionUpdated(session, now));
            }

            _restrictions.Remove(restriction);

            AddAggregateChangeEvent(new UserRestrictionUpdated(restriction, now));

            return Result.Success();
        }

        public Result RemoveRestrictionSession(RestrictionType restrictionType, DateTimeOffset now)
        {
            var session = _restrictionSessions
                .FirstOrDefault(x => x.RestrictionType == restrictionType);

            if (session is null)
                return Result.Failure(RestrictionErrors.NotFoundSessionType<User>(restrictionType.ToString()));

            var restrictions = _restrictions
                .Where(x =>
                    !x.IsRevoked &&
                    x.RestrictionType == restrictionType)
                .ToList();

            foreach (var restriction in restrictions)
            {
                if (!session.ContainsRestriction(restriction.Id))
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Restriction session is inconsistent with active restrictions!",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id,
                            ["RestrictionId"] = restriction.Id
                        });
                }
            }

            foreach (var restriction in restrictions)
            {
                var result = restriction.CancelRestriction(now);

                if (result.IsFailure)
                    return result;

                _restrictions.Remove(restriction);

                AddAggregateChangeEvent(new UserRestrictionUpdated(restriction, now));
            }

            _restrictionSessions.Remove(session);

            AddAggregateChangeEvent(new UserRestrictionSessionDeleted(session.Id, now));

            return Result.Success();
        }

        private Result<List<UserRestriction>> CreateRestrictions(IEnumerable<RestrictionData> restrictionDataCollection)
        {
            var restrictions = new List<UserRestriction>();

            foreach (var data in restrictionDataCollection)
            {
                var result = UserRestriction.Create(
                    Id,
                    data.RestrictionType,
                    data.RestrictionRule,
                    data.RestrictedBy,
                    data.Reason);

                if (result.IsFailure)
                    return Result<List<UserRestriction>>.Failure(result.Errors);

                restrictions.Add(result.Value);
            }

            return Result<List<UserRestriction>>.Success(restrictions);
        }

        private void AddRestrictionCreatedEvents(IEnumerable<UserRestriction> userRestrictions, DateTimeOffset now)
        {
            foreach (var restriction in userRestrictions)
            {
                AddAggregateChangeEvent(new UserRestrictionCreated(restriction, now));
            }
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