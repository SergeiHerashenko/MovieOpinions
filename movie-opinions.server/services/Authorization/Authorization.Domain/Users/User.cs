using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
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

        #region Behavior (Action)
        public Result<UserPendingAction> ActionChangeLogin(Login newLogin, DateTimeOffset now)
        {
            if (newLogin is null)
                return Result<UserPendingAction>.Failure(LoginErrors.EmptyLogin<User>());

            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            if (_action is not null)
                return Result<UserPendingAction>.Failure(ActionErrors.ActionAlreadyExists<User>());

            if (newLogin == Login)
                return Result<UserPendingAction>.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(Login)));

            var actionChengeLogin = UserAction.From(newLogin);

            var createActionResult = CreateAction(actionChengeLogin, now);

            if (createActionResult.IsFailure)
                return createActionResult;

            var createdAction = createActionResult.Value;

            AddAggregateChange(new UserPendingActionCreated(createActionResult.Value, now));

            return Result<UserPendingAction>.Success(createdAction);
        }

        public Result<UserPendingAction> ActionChangePassword(Password newPassword, DateTimeOffset now)
        {
            if (newPassword is null)
                return Result<UserPendingAction>.Failure(PasswordErrors.EmptyHashPassword<User>());

            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            if (_action is not null)
                return Result<UserPendingAction>.Failure(ActionErrors.ActionAlreadyExists<User>());

            if (newPassword == Password)
                return Result<UserPendingAction>.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(Password)));

            var actionChangePassword = UserAction.From(newPassword);

            var createActionResult = CreateAction(actionChangePassword, now);

            if (createActionResult.IsFailure)
                return createActionResult;

            var createdAction = createActionResult.Value;

            AddAggregateChange(new UserPendingActionCreated(createActionResult.Value, now));

            return Result<UserPendingAction>.Success(createdAction);
        }

        public Result<UserPendingAction> ActionDeletingUser(string? reason, DateTimeOffset now)
        {
            if (IsDeleted)
                return Result<UserPendingAction>.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var deletionReason = DeletionReason.Create(reason);

            if (deletionReason.IsFailure)
                return Result<UserPendingAction>.Failure(deletionReason.Errors);

            ExpirePendingActionIfNeeded(now);

            if (_action is not null)
                return Result<UserPendingAction>.Failure(ActionErrors.ActionAlreadyExists<User>());

            var actionDeletingUser = UserAction.From(deletionReason.Value);

            var createActionDeletingResult = CreateAction(actionDeletingUser, now);

            if (createActionDeletingResult.IsFailure)
                return createActionDeletingResult;

            var createdAction = createActionDeletingResult.Value;

            AddAggregateChange(new UserPendingActionCreated(createActionDeletingResult.Value, now));

            return Result<UserPendingAction>.Success(createdAction);
        }

        public Result FailPendingAction(UserPendingActionId userPendingActionId, DateTimeOffset now)
        {
            if (_action is null)
                return Result.Failure(DeletionErrors.NotDeleteUser<User>());

            if (_action.Id != userPendingActionId)
                return Result.Failure(DeletionErrors.NotFoundAction<User>());

            var failResult = _action.MarkAsFailed(userPendingActionId);

            if (failResult.IsFailure)
                return failResult;

            AddAggregateChange(new UserPendingActionUpdated(_action, now));

            _action = null;

            return Result.Success();
        }

        private void ExpirePendingActionIfNeeded(DateTimeOffset now)
        {
            if (_action is null)
                return;

            if (_action.ExpiresAt > now)
                return;

            var expiredAction = _action;

            expiredAction.MarkAsExpired(now);

            AddAggregateChange(new UserPendingActionUpdated(expiredAction, now));

            _action = null;
        }

        private Result<UserPendingAction> CreateAction(UserAction userChange, DateTimeOffset now)
        {
            var actionResult = UserPendingAction.Create(Id, userChange, now);

            if (actionResult.IsFailure)
                return actionResult;

            _action = actionResult.Value;

            AddDomainEvent(new UserPendingActionEvent(
                actionResult.Value.Id,
                Login,
                actionResult.Value.UserAction,
                actionResult.Value.ExpiresAt,
                now)
            );

            return Result<UserPendingAction>.Success(actionResult.Value);
        }
        #endregion

        #region Behavior (Confirm Action)
        public Result ConfirmLogin(string confirmationToken, DateTimeOffset now)
        {
            return CompleteAction<LoginChangeAction>(
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
            return CompleteAction<PasswordChangeAction>(
                confirmationToken,
                now,
                change =>
                {
                    Password = change.NewPassword;
                }
            );
        }

        public Result ConfirmDeleting(string confirmationToken, DateTimeOffset now)
        {
            return CompleteAction<DeleteAccountAction>(
                confirmationToken,
                now,
                change =>
                {
                    Delete(now, change.Reason);
                }
            );
        }

        private Result CompleteAction<TAction>(
            string confirmationToken,
            DateTimeOffset now,
            Action<TAction> applyAction)
            where TAction : UserAction
        {
            var access = ProvideAccess();

            if (access.IsFailure)
                return access;

            if (_action is null)
                return Result.Failure(ActionErrors.EmptyAction<User>());

            var confirmResult = _action.ConfirmAction(confirmationToken, now);

            if (confirmResult.IsFailure)
                return confirmResult;

            var action = _action.UserAction as TAction;

            if (action is null)
                return Result.Failure(ActionErrors.InvalidActionType<User>());

            applyAction(action);

            UpdatedAt = now;

            var actionChange = _action;

            _action = null;

            AddAggregateChange(new UserPendingActionUpdated(actionChange, now));
            AddDomainEvent(new UserActionEvent(
                actionChange.Id,
                Login,
                actionChange.UserAction,
                actionChange.ExpiresAt,
                now)
            );

            return Result.Success();
        }

        private Result Delete(DateTimeOffset now, DeletionReason reason)
        {
            if (IsDeleted)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var createDeletionResult = UserDeletion.Create(Id, Login, now, reason);

            if (createDeletionResult.IsFailure)
                return createDeletionResult;

            var deletion = createDeletionResult.Value;

            _deletion = deletion;

            AddAggregateChange(new UserDeletionCreated(deletion, now));

            return Result.Success();
        }
        #endregion

        #region Behavior (Get)
        public Result<UserPendingAction> GetPendingAction()
        {
            if (_action is null)
                return Result<UserPendingAction>.Failure(ActionErrors.EmptyAction<User>());

            return Result<UserPendingAction>.Success(_action);
        }

        public Result<UserDeletion> GetDeletion()
        {
            if (_deletion is null)
                return Result<UserDeletion>.Failure(DeletionErrors.NotDeleteUser<User>());

            return Result<UserDeletion>.Success(_deletion);
        }

        public Result<UserPendingAction> GetDeletionActionForConfirmation(ConfirmationToken confirmationToken, DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            if (_action is null)
                return Result<UserPendingAction>.Failure(ActionErrors.EmptyAction<User>());

            if (_action.ExpiresAt <= now)
                return Result<UserPendingAction>.Failure(ActionErrors.Expired<User>());

            if (_action.ConfirmationToken != confirmationToken)
                return Result<UserPendingAction>.Failure(ActionErrors.InvalidConfirmationToken<User>());

            if (_action.UserAction is not DeleteAccountAction)
                return Result<UserPendingAction>.Failure(ActionErrors.InvalidActionType<User>());

            return Result<UserPendingAction>.Success(_action);
        }
        #endregion

        #region Behavior
        public Result RecordFailedPasswordAttempt(DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            FailedPasswordAttempts++;
            UpdatedAt = now;

            if(FailedPasswordAttempts > MaxFailedAttempts)
            {
                var restrictionData = CreateFailedLoginBanData();

                if (restrictionData.IsFailure)
                    return restrictionData;

                var addResult = AddRestrictions(new[] { restrictionData.Value }, now);

                if (addResult.IsFailure)
                    return addResult;

                FailedPasswordAttempts = 0;
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

            FailedPasswordAttempts = 0;
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

            AddAggregateChange(new UserRefreshTokenUpdated(
                refreshToken.Id,
                Id,
                refreshToken.TokenStatus,
                refreshToken.ConsumedAt,
                refreshToken.RevokedAt,
                now)
            );

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

            AddAggregateChange(new UserRefreshTokenUpdated(
                refreshToken.Id,
                Id,
                refreshToken.TokenStatus,
                refreshToken.ConsumedAt,
                refreshToken.RevokedAt,
                now)
            );

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

        #region Behavior (Removal)
        public Result Undelete(DateTimeOffset now)
        {
            if(_deletion is null)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var result = _deletion.Undelete(now);

            if(result.IsFailure)
                return result;

            AddDomainEvent(new UserUndeletedEvent(_deletion.Id, Login, now));

            AddAggregateChange(new UserDeletionUpdated(_deletion, now));

            return Result.Success();
        }

        public bool UpdateExpirationStatus(DateTimeOffset now)
        {
            if (_deletion is null)
                return false;

            if (!_deletion.MarkAsExpired(now))
                return false;

            AddAggregateChange(new UserDeletionUpdated(_deletion, now));

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

                var restrictionDescription = restrictionDataCollection
                    .Select(x => (Rule: x.RestrictionRule, Reason: x.Reason))
                    .ToArray();

                if (session is null)
                {
                    var createSessionResult = UserRestrictionSession.Create(Id, restrictionGroup);

                    if (createSessionResult.IsFailure)
                        return createSessionResult;

                    _restrictionSessions.Add(createSessionResult.Value);

                    AddDomainEvent(new UserRestrictionSessionCreatedEvent(
                        createSessionResult.Value.Id,
                        Login,
                        restrictionDescription,
                        createSessionResult.Value.RestrictionType,
                        createSessionResult.Value.TotalBlockedMinutes,
                        now)
                    );

                    AddAggregateChange(new UserRestrictionSessionCreated(createSessionResult.Value, now));

                    continue;
                }

                var addRestrictionsResult = session.AddRestrictions(restrictionGroup);

                if (addRestrictionsResult.IsFailure)
                    return addRestrictionsResult;

                AddDomainEvent(new UserRestrictionSessionAddRestrictionEvent(
                    session.Id,
                    Login,
                    restrictionDescription,
                    session.RestrictionType,
                    session.TotalBlockedMinutes,
                    now)
                );

                AddAggregateChange(new UserRestrictionSessionUpdated(session, now));
            }

            _restrictions.AddRange(createRestrictionsResult.Value);

            AddRestrictionCreatedChange(createRestrictionsResult.Value, now);

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

                AddAggregateChange(new UserRestrictionSessionDeleted(session.Id, now));
            }
            else
            {
                AddAggregateChange(new UserRestrictionSessionUpdated(session, now));
            }

            _restrictions.Remove(restriction);

            AddDomainEvent(new UserRestrictionSessionRemovedRestrictionEvent(
                session.Id,
                Login,
                restriction.RestrictionRule,
                restriction.RestrictionType,
                session.TotalBlockedMinutes,
                now)
            );

            AddAggregateChange(new UserRestrictionUpdated(restriction, now));

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

                AddAggregateChange(new UserRestrictionUpdated(restriction, now));
            }

            _restrictionSessions.Remove(session);

            AddDomainEvent(new UserRestrictionSessionRemovedEvent(
                session.Id,
                Login,
                session.RestrictionType,
                now)
            );

            AddAggregateChange(new UserRestrictionSessionDeleted(session.Id, now));

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

        private void AddRestrictionCreatedChange(IEnumerable<UserRestriction> userRestrictions, DateTimeOffset now)
        {
            foreach (var restriction in userRestrictions)
            {
                AddAggregateChange(new UserRestrictionCreated(restriction, now));
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