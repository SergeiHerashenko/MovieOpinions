using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction
{
    public class UserPendingActionTests
    {
        #region Test data
        // Фіксовані моменти роблять перевірки меж строку дії відтворюваними.
        // Fixed instants make lifetime boundary checks reproducible.
        private static readonly DateTimeOffset T0 =
            new(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset T1 = T0.AddMinutes(1);
        private static readonly DateTimeOffset D = T0.AddMinutes(30);
        #endregion

        #region Helpers: factories
        private static UserId CreateUserId()
            => UserId.Restore(Guid.Parse("11111111-1111-4111-8111-111111111111"));

        private static UserPendingActionId CreatePendingActionId()
            => UserPendingActionId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222"));

        private static ConfirmationFlowToken CreateToken(byte fill = 0)
        {
            var bytes = new byte[64];

            Array.Fill(
                bytes,
                fill
            );

            var result = ConfirmationFlowToken.Parse(Convert.ToBase64String(bytes));

            Assert.True(result.IsSuccess);

            return result.Value;
        }

        private static UserAction CreatePasswordAction(string hash = "TEST.HASH")
            => UserAction.From(Password.Create(PasswordHash.Create(hash)));

        private static UserAction CreateEmailAction(string address = "serhii@example.com")
            => UserAction.From(Login.From(Email.Restore(address)));

        private static UserAction CreatePhoneAction()
            => UserAction.From(Login.From(
                Phone.Restore(
                    PhoneCountryCode.Restore("+380"),
                    PhoneNationalNumber.Restore("981234567"))
                )
            );

        private static UserAction CreateDeleteAction()
        {
            var reason = DeletionReason.Create("Account closed");

            Assert.True(reason.IsSuccess);

            return UserAction.From(reason.Value);
        }

        private static UserPendingAction CreateActive(
            UserAction? action = null,
            DateTimeOffset? now = null)
            => UserPendingAction.Create(
                CreateUserId(),
                action ?? CreatePasswordAction(),
                now ?? T0
            );

        private static UserPendingAction RestoreActive(
            DateTimeOffset? createdAt = null,
            DateTimeOffset? expiresAt = null,
            UserAction? action = null,
            ConfirmationFlowToken? token = null)
            => UserPendingAction.Restore(
                CreatePendingActionId(),
                CreateUserId(),
                token ?? CreateToken(),
                action ?? CreatePasswordAction(),
                expiresAt ?? (createdAt ?? T0).AddMinutes(30),
                null, null, null,
                ActionStatus.Active,
                createdAt ?? T0
            );

        private static UserPendingAction RestoreConfirmed(DateTimeOffset confirmationTime)
            => UserPendingAction.Restore(
                CreatePendingActionId(),
                CreateUserId(),
                CreateToken(),
                CreatePasswordAction(),
                D, confirmationTime, null, null,
                ActionStatus.Confirmed, T0
            );

        private static UserPendingAction RestoreCancelled(DateTimeOffset cancelledTime)
            => UserPendingAction.Restore(
                CreatePendingActionId(),
                CreateUserId(),
                CreateToken(),
                CreatePasswordAction(),
                D, null, null, cancelledTime,
                ActionStatus.Cancelled, T0
            );

        private static UserPendingAction RestoreExpired(DateTimeOffset expiredAt)
            => UserPendingAction.Restore(
                CreatePendingActionId(),
                CreateUserId(),
                CreateToken(),
                CreatePasswordAction(),
                D, null, expiredAt, null,
                ActionStatus.Expired, T0
            );
        #endregion

        #region Helpers: state assertions
        private static void AssertUnchanged(
            UserPendingAction entity,
            UserPendingActionId expectedId,
            UserId expectedUserId,
            ConfirmationFlowToken expectedToken,
            UserAction expectedAction,
            DateTimeOffset expectedCreatedAt,
            DateTimeOffset expectedExpiresAt,
            ActionStatus expectedStatus,
            DateTimeOffset? expectedConfirmationTime,
            DateTimeOffset? expectedExpiredAt,
            DateTimeOffset? expectedCancelledTime)
        {
            Assert.Same(
                expectedId,
                entity.Id
            );

            Assert.Same(
                expectedUserId,
                entity.UserId
            );

            Assert.Same(
                expectedToken,
                entity.ConfirmationToken
            );

            Assert.Same(
                expectedAction,
                entity.UserAction
            );

            Assert.Equal(
                expectedCreatedAt,
                entity.CreatedAt
            );

            Assert.Equal(
                expectedExpiresAt,
                entity.ExpiresAt
            );

            Assert.Equal(
                expectedStatus,
                entity.Status
            );

            Assert.Equal(
                expectedConfirmationTime,
                entity.ConfirmationTime
            );

            Assert.Equal(
                expectedExpiredAt,
                entity.ExpiredAt
            );

            Assert.Equal(
                expectedCancelledTime,
                entity.CancelledTime
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        // Знімок властивостей робиться до виклику, а не після отримання результату.
        // Properties are captured before the call rather than after obtaining its result.
        private static void CaptureAndAssertUnchanged(
            UserPendingAction entity,
            Action act)
        {
            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedToken = entity.ConfirmationToken;
            var expectedAction = entity.UserAction;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedExpiresAt = entity.ExpiresAt;
            var expectedStatus = entity.Status;
            var expectedConfirmationTime = entity.ConfirmationTime;
            var expectedExpiredAt = entity.ExpiredAt;
            var expectedCancelledTime = entity.CancelledTime;

            act();

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedToken,
                expectedAction,
                expectedCreatedAt,
                expectedExpiresAt,
                expectedStatus,
                expectedConfirmationTime,
                expectedExpiredAt,
                expectedCancelledTime
            );
        }
        #endregion

        #region Helpers: exception assertions
        private static void AssertEmptyField(
            DomainDataInconsistencyException exception,
            string field,
            string operation)
        {
            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );
        }

        private static void AssertOutOfRangeNow(
            DomainDataInconsistencyException exception,
            DateTimeOffset createdAt)
        {
            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "now",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "DateTimeOffset",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                "CreatedAt",
                exception.Context["Custom_MinimumField"]
            );

            Assert.Equal(
                createdAt,
                exception.Context["Custom_MinimumValue"]
            );
        }

        private static void AssertTerminalInvariant(
            DomainInvariantViolationException exception,
            Guid pendingActionId,
            Guid userId,
            ActionStatus status,
            DateTimeOffset expiresAt,
            DateTimeOffset now)
        {
            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Only an active pending action can transition to another state.",
                exception.Context["Rule"]
            );

            Assert.Equal(
                pendingActionId,
                exception.Context["State_PendingActionId"]
            );

            Assert.Equal(
                userId,
                exception.Context["State_UserId"]
            );

            Assert.Equal(
                status,
                exception.Context["State_ActionStatus"]
            );

            Assert.Equal(
                expiresAt,
                exception.Context["State_ExpiresAt"]
            );

            Assert.Equal(
                now,
                exception.Context["State_CurrentTime"]
            );
        }

        private static void AssertDateConflictInvariant(
            DomainInvariantViolationException exception,
            UserPendingActionId expectedId,
            ActionStatus expectedStatus,
            DateTimeOffset expectedCreatedAt,
            DateTimeOffset expectedExpiresAt,
            DateTimeOffset? expectedConfirmationTime,
            DateTimeOffset? expectedExpiredAt,
            DateTimeOffset? expectedCancelledTime)
        {
            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                $"Dates conflict. ConfirmationTime: {expectedConfirmationTime?.ToString() ?? "null"}, " +
                $"ExpiredAt: {expectedExpiredAt?.ToString() ?? "null"}",
                exception.Context["Rule"]
            );

            Assert.Same(
                expectedId,
                exception.Context["State_PendingActionId"]
            );

            Assert.Equal(
                expectedStatus,
                exception.Context["State_ActionStatus"]
            );

            Assert.Equal(
                expectedCreatedAt,
                exception.Context["State_CreatedAt"]
            );

            Assert.Equal(
                expectedExpiresAt,
                exception.Context["State_ExpiresAt"]
            );

            Assert.Equal(
                (object?)expectedConfirmationTime ?? "null",
                exception.Context["State_ConfirmationTime"]
            );

            Assert.Equal(
                (object?)expectedExpiredAt ?? "null",
                exception.Context["State_ExpiredAt"]
            );

            Assert.Equal(
                (object?)expectedCancelledTime ?? "null",
                exception.Context["State_CancelledTime"]
            );
        }

        private static void AssertMarkPreconditionFailed(
            DomainInvalidOperationException exception,
            bool isConfirm,
            UserPendingAction entity,
            DateTimeOffset now,
            string expectedErrorCode)
        {
            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                isConfirm ? "MarkAsConfirmed" : "MarkAsCancelled",
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                isConfirm
                    ? "A successfully validated pending-action confirmation."
                    : "A successfully validated pending-action cancellation.",
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                entity.Id.Value,
                exception.Context["Custom_PendingActionId"]
            );

            Assert.Equal(
                entity.UserId.Value,
                exception.Context["Custom_UserId"]
            );

            Assert.Equal(
                ActionStatus.Active,
                exception.Context["Custom_ActionStatus"]
            );

            Assert.Equal(
                entity.ExpiresAt,
                exception.Context["Custom_ExpiresAt"]
            );

            Assert.Equal(
                now,
                exception.Context["Custom_CurrentTime"]
            );

            var codes = Assert.IsType<string[]>(
                exception.Context["Custom_ValidationErrorCodes"]);

            Assert.Equal(
                new[] { expectedErrorCode },
                codes
            );
        }

        private static void AssertExpirePreconditionFailed(
            DomainInvalidOperationException exception,
            UserPendingAction entity,
            DateTimeOffset now)
        {
            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );
            Assert.Equal(
                "ValidateCanExpire",
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                "The pending action expiration time must be reached.",
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                entity.Id.Value,
                exception.Context["Custom_PendingActionId"]
            );

            Assert.Equal(
                entity.ExpiresAt,
                exception.Context["Custom_ExpiresAt"]
            );

            Assert.Equal(
                now,
                exception.Context["Custom_CurrentTime"]
            );
        }
        #endregion

        #region Create
        [Theory]
        [InlineData("password")]
        [InlineData("email")]
        [InlineData("phone")]
        [InlineData("delete")]
        public void Create_ValidArguments_SetsInitialState(string actionKind)
        {
            var userId = CreateUserId();

            var action = actionKind switch
            {
                "email" => CreateEmailAction(),
                "phone" => CreatePhoneAction(),
                "delete" => CreateDeleteAction(),
                _ => CreatePasswordAction()
            };
            var now = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var entity = UserPendingAction.Create(userId, action, now);

            Assert.NotEqual(
                Guid.Empty,
                entity.Id.Value
            );

            Assert.Equal(
                7,
                entity.Id.Value.Version
            );

            Assert.Same(
                userId,
                entity.UserId
                );
            Assert.Same(
                action,
                entity.UserAction
            );

            Assert.False(string.IsNullOrWhiteSpace(entity.ConfirmationToken.Value));

            Assert.Equal(
                88,
                entity.ConfirmationToken.Value.Length
            );

            Assert.Equal(
                64,
                Convert.FromBase64String(entity.ConfirmationToken.Value).Length
            );

            Assert.Equal(
                now,
                entity.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                entity.CreatedAt.Offset
            );

            Assert.Equal(
                now.AddMinutes(30),
                entity.ExpiresAt
            );

            Assert.Equal(
                now.Offset,
                entity.ExpiresAt.Offset
            );

            Assert.Equal(
                ActionStatus.Active,
                entity.Status
            );

            Assert.Null(entity.ConfirmationTime);
            Assert.Null(entity.ExpiredAt);
            Assert.Null(entity.CancelledTime);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Fact]
        public void Create_SeparateCalls_GenerateDifferentIdsAndTokens()
        {
            var first = CreateActive();
            var second = CreateActive();

            Assert.NotEqual(
                first.Id.Value,
                second.Id.Value
            );

            Assert.NotEqual(
                first.ConfirmationToken.Value,
                second.ConfirmationToken.Value
            );
        }

        [Theory]
        [InlineData("userId")]
        [InlineData("userAction")]
        public void Create_NullRequiredArgument_ThrowsDomainDataInconsistencyException(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingAction.Create(
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "userAction" ? null! : CreatePasswordAction(),
                    T0
                )
            );

            AssertEmptyField(
                exception,
                nullField,
                "Create"
            );
        }

        [Fact]
        public void Create_BothArgumentsNull_ReportsUserId()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingAction.Create(null!, null!, T0));

            AssertEmptyField(
                exception,
                "userId",
                "Create"
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ExtremeSupportedTimestamps_Succeeds(bool useMinValue)
        {
            var now = useMinValue
                ? DateTimeOffset.MinValue
                : DateTimeOffset.MaxValue.AddMinutes(-30);

            var entity = UserPendingAction.Create(
                CreateUserId(),
                CreatePasswordAction(),
                now
            );

            Assert.Equal(
                now,
                entity.CreatedAt
            );

            Assert.Equal(
                now.AddMinutes(30),
                entity.ExpiresAt
            );

            if (!useMinValue)
            {
                Assert.Equal(
                    DateTimeOffset.MaxValue,
                    entity.ExpiresAt
                );
            }

        }

        [Fact]
        public void Create_ExpirationOverflows_ThrowsArgumentOutOfRangeException()
        {
            var now = DateTimeOffset.MaxValue.AddMinutes(-30).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UserPendingAction.Create(
                    CreateUserId(), CreatePasswordAction(), now
                )
            );
        }
        #endregion

        #region Restore: arguments and stored state
        [Theory]
        [InlineData(ActionStatus.Active)]
        [InlineData(ActionStatus.Confirmed)]
        [InlineData(ActionStatus.Cancelled)]
        [InlineData(ActionStatus.Expired)]
        public void Restore_ValidState_PreservesAllProperties(ActionStatus status)
        {
            var id = CreatePendingActionId();
            var userId = CreateUserId();
            var token = CreateToken();
            var action = CreatePasswordAction();
            DateTimeOffset? confirmationTime = status == ActionStatus.Confirmed ? T1 : null;
            DateTimeOffset? expiredAt = status == ActionStatus.Expired ? D : null;
            DateTimeOffset? cancelledTime = status == ActionStatus.Cancelled ? T1 : null;

            var entity = UserPendingAction.Restore(
                id, userId, token, action, D,
                confirmationTime, expiredAt, cancelledTime,
                status, T0
            );

            Assert.Same(
                id,
                entity.Id
            );

            Assert.Same(
                userId,
                entity.UserId
            );

            Assert.Same(
                token,
                entity.ConfirmationToken
            );

            Assert.Same(
                action,
                entity.UserAction
            );

            Assert.Equal(
                T0,
                entity.CreatedAt
            );

            Assert.Equal(
                D,
                entity.ExpiresAt
            );

            Assert.Equal(
                status,
                entity.Status
            );

            Assert.Equal(
                confirmationTime,
                entity.ConfirmationTime
            );

            Assert.Equal(
                expiredAt,
                entity.ExpiredAt
            );

            Assert.Equal(
                cancelledTime,
                entity.CancelledTime
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerMinute)]
        [InlineData(30 * TimeSpan.TicksPerMinute)]
        [InlineData(60 * TimeSpan.TicksPerMinute)]
        public void Restore_CustomLifetime_PreservesStoredExpiration(long durationTicks)
        {
            var expiresAt = T0.AddTicks(durationTicks);

            var entity = RestoreActive(expiresAt: expiresAt);

            Assert.Equal(
                expiresAt,
                entity.ExpiresAt
            );
        }

        [Theory]
        [InlineData("userPendingActionId")]
        [InlineData("userId")]
        [InlineData("confirmationToken")]
        [InlineData("userAction")]
        public void Restore_NullRequiredArgument_ThrowsDomainDataInconsistencyException(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingAction.Restore(
                    nullField == "userPendingActionId" ? null! : CreatePendingActionId(),
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "confirmationToken" ? null! : CreateToken(),
                    nullField == "userAction" ? null! : CreatePasswordAction(),
                    D, null, null, null, ActionStatus.Active, T0
                )
            );

            AssertEmptyField(
                exception,
                nullField,
                "Restore"
            );
        }

        [Theory]
        [InlineData(true, true, true, true, "userPendingActionId")]
        [InlineData(false, true, true, true, "userId")]
        [InlineData(false, false, true, true, "confirmationToken")]
        public void Restore_MultipleNullArguments_ReportsFirstNullField(
            bool nullId,
            bool nullUserId,
            bool nullToken,
            bool nullAction,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingAction.Restore(
                    nullId ? null! : CreatePendingActionId(),
                    nullUserId ? null! : CreateUserId(),
                    nullToken ? null! : CreateToken(),
                    nullAction ? null! : CreatePasswordAction(),
                    D, null, null, null, ActionStatus.Active, T0
                )
            );

            AssertEmptyField(
                exception,
                expectedField,
                "Restore"
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Restore_UndefinedStatus_ThrowsDomainDataInconsistencyException(int statusValue)
        {
            var status = (ActionStatus)statusValue;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingAction.Restore(
                    CreatePendingActionId(), CreateUserId(), CreateToken(),
                    CreatePasswordAction(), D, null, null, null, status, T0
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "actionStatus",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                status,
                exception.Context["Value"]
            );
        }
        #endregion

        #region Restore: lifecycle timestamps
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Restore_ExpirationNotAfterCreation_ThrowsDomainInvariantViolationException(long ticksOffset)
        {
            var expiresAt = ticksOffset == 0 ? T0 : T0.AddTicks(-1);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingAction.Restore(
                    CreatePendingActionId(), CreateUserId(), CreateToken(),
                    CreatePasswordAction(), expiresAt, null, null, null,
                    ActionStatus.Active, T0
                )
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "The expiration time 'expiresAt' cannot be less than " +
                "(or equal to) the creation time 'createdAt'!",
                exception.Context["Rule"]
            );

            Assert.Equal(
                expiresAt,
                exception.Context["State_ExpiresAt"]
            );

            Assert.Equal(
                T0,
                exception.Context["State_CreatedAt"]
            );
        }

        [Fact]
        public void Restore_ExpirationEqualInstantDifferentOffset_ThrowsDomainInvariantViolationException()
        {
            var expiresAt = T0.ToOffset(TimeSpan.FromHours(2));

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingAction.Restore(
                    CreatePendingActionId(), CreateUserId(), CreateToken(),
                    CreatePasswordAction(), expiresAt, null, null, null,
                    ActionStatus.Active, T0
                )
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserPendingAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "The expiration time 'expiresAt' cannot be less than " +
                "(or equal to) the creation time 'createdAt'!",
                exception.Context["Rule"]
            );

            Assert.Equal(
                expiresAt,
                exception.Context["State_ExpiresAt"]
            );

            Assert.Equal(
                T0,
                exception.Context["State_CreatedAt"]
            );
        }

        // Генеруємо всі несумісні поєднання статусу та наявності кінцевих часових міток.
        // Generate every inconsistent combination of status and terminal timestamp presence.
        public static TheoryData<ActionStatus, bool, bool, bool> InconsistentPresenceCases()
        {
            var data = new TheoryData<ActionStatus, bool, bool, bool>();

            foreach (var status in new[]
                     {
                        ActionStatus.Active,
                        ActionStatus.Confirmed,
                        ActionStatus.Expired,
                        ActionStatus.Cancelled
                     })
            {
                for (var c = 0; c <= 1; c++)
                {
                    for (var e = 0; e <= 1; e++)
                    {
                        for (var x = 0; x <= 1; x++)
                        {
                            var hasC = c == 1;
                            var hasE = e == 1;
                            var hasX = x == 1;

                            var isValid = (status, hasC, hasE, hasX) switch
                            {
                                (ActionStatus.Active, false, false, false) => true,
                                (ActionStatus.Confirmed, true, false, false) => true,
                                (ActionStatus.Expired, false, true, false) => true,
                                (ActionStatus.Cancelled, false, false, true) => true,
                                _ => false
                            };

                            if (!isValid)
                            {
                                data.Add(
                                    status,
                                    hasC,
                                    hasE,
                                    hasX
                                );
                            }
                        }
                    }
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(InconsistentPresenceCases))]
        public void Restore_InconsistentTimestampPresence_ThrowsDomainInvariantViolationException(
            ActionStatus status,
            bool hasConfirmation,
            bool hasExpired,
            bool hasCancelled)
        {
            DateTimeOffset? confirmationTime = hasConfirmation ? T1 : null;
            DateTimeOffset? expiredAt = hasExpired ? D : null;
            DateTimeOffset? cancelledTime = hasCancelled ? T1 : null;

            var id = CreatePendingActionId();

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingAction.Restore(
                    id, CreateUserId(), CreateToken(), CreatePasswordAction(),
                    D, confirmationTime, expiredAt, cancelledTime, status, T0
                )
            );

            AssertDateConflictInvariant(
                exception,
                id,
                status,
                T0,
                D,
                confirmationTime,
                expiredAt,
                cancelledTime
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void Restore_ConfirmedAtAllowedBoundary_Succeeds(long ticksFromBoundary)
        {
            var confirmationTime = ticksFromBoundary switch
            {
                0 => T0,
                1 => T0.AddTicks(1),
                _ => D.AddTicks(-1)
            };

            var entity = RestoreConfirmed(confirmationTime);

            Assert.Equal(
                ActionStatus.Confirmed,
                entity.Status
            );

            Assert.Equal(
                confirmationTime,
                entity.ConfirmationTime
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void Restore_ConfirmedOutsideAllowedWindow_ThrowsDomainInvariantViolationException(long ticksFromBoundary)
        {
            var confirmationTime = ticksFromBoundary switch
            {
                -1 => T0.AddTicks(-1),
                0 => D,
                _ => D.AddTicks(1)
            };

            var id = CreatePendingActionId();

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingAction.Restore(
                    id, CreateUserId(), CreateToken(), CreatePasswordAction(),
                    D, confirmationTime, null, null, ActionStatus.Confirmed, T0
                )
            );

            AssertDateConflictInvariant(
                exception,
                id,
                ActionStatus.Confirmed,
                T0,
                D,
                confirmationTime,
                null,
                null
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void Restore_CancelledAtAllowedBoundary_Succeeds(long ticksFromBoundary)
        {
            var cancelledTime = ticksFromBoundary switch
            {
                0 => T0,
                1 => T0.AddTicks(1),
                _ => D.AddTicks(-1)
            };

            var entity = RestoreCancelled(cancelledTime);

            Assert.Equal(
                ActionStatus.Cancelled,
                entity.Status
            );

            Assert.Equal(
                cancelledTime,
                entity.CancelledTime
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void Restore_CancelledOutsideAllowedWindow_ThrowsDomainInvariantViolationException(long ticksFromBoundary)
        {
            var cancelledTime = ticksFromBoundary switch
            {
                -1 => T0.AddTicks(-1),
                0 => D,
                _ => D.AddTicks(1)
            };

            var id = CreatePendingActionId();

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingAction.Restore(
                    id, CreateUserId(), CreateToken(), CreatePasswordAction(),
                    D, null, null, cancelledTime, ActionStatus.Cancelled, T0
                )
            );

            AssertDateConflictInvariant(
                exception,
                id,
                ActionStatus.Cancelled,
                T0,
                D,
                null,
                null,
                cancelledTime
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void Restore_ExpiredAtOrAfterDeadline_Succeeds(long ticksFromDeadline)
        {
            var expiredAt = D.AddTicks(ticksFromDeadline);

            var entity = RestoreExpired(expiredAt);

            Assert.Equal(
                ActionStatus.Expired,
                entity.Status
            );

            Assert.Equal(
                expiredAt,
                entity.ExpiredAt
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(-TimeSpan.TicksPerMinute * 30)]
        public void Restore_ExpiredBeforeDeadline_ThrowsDomainInvariantViolationException(long ticksFromDeadline)
        {
            var expiredAt = ticksFromDeadline switch
            {
                -1 => D.AddTicks(-1),
                0 => T0,
                _ => T0.AddTicks(-1)
            };

            var id = CreatePendingActionId();

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingAction.Restore(
                    id, CreateUserId(), CreateToken(), CreatePasswordAction(),
                    D, null, expiredAt, null, ActionStatus.Expired, T0
                )
            );

            AssertDateConflictInvariant(
                exception,
                id,
                ActionStatus.Expired,
                T0,
                D,
                null,
                expiredAt,
                null
            );
        }

        [Fact]
        public void Restore_DifferentOffsets_UsesActualInstantAndPreservesOffsets()
        {
            var confirmationTime = T0.ToOffset(TimeSpan.FromHours(2));
            var confirmed = RestoreConfirmed(confirmationTime);

            Assert.Equal(
                confirmationTime,
                confirmed.ConfirmationTime
            );

            Assert.Equal(
                confirmationTime.Offset,
                confirmed.ConfirmationTime!.Value.Offset
            );

            var cancelledTime = T0.ToOffset(TimeSpan.FromHours(-3));
            var cancelled = RestoreCancelled(cancelledTime);

            Assert.Equal(
                cancelledTime,
                cancelled.CancelledTime
            );

            Assert.Equal(
                cancelledTime.Offset,
                cancelled.CancelledTime!.Value.Offset
            );

            var expiredAt = D.ToOffset(TimeSpan.FromHours(5));
            var expired = RestoreExpired(expiredAt);

            Assert.Equal(
                expiredAt,
                expired.ExpiredAt
            );

            Assert.Equal(
                expiredAt.Offset,
                expired.ExpiredAt!.Value.Offset
            );
        }

        [Theory]
        [InlineData(ActionStatus.Active)]
        [InlineData(ActionStatus.Confirmed)]
        [InlineData(ActionStatus.Cancelled)]
        [InlineData(ActionStatus.Expired)]
        public void Restore_ExtremeValidTimestamps_PreservesValues(ActionStatus status)
        {
            DateTimeOffset? confirmationTime = status == ActionStatus.Confirmed
                ? DateTimeOffset.MinValue : null;

            DateTimeOffset? cancelledTime = status == ActionStatus.Cancelled
                ? DateTimeOffset.MinValue : null;

            DateTimeOffset? expiredAt = status == ActionStatus.Expired
                ? DateTimeOffset.MaxValue : null;

            var entity = UserPendingAction.Restore(
                CreatePendingActionId(), CreateUserId(), CreateToken(),
                CreatePasswordAction(),
                DateTimeOffset.MaxValue,
                confirmationTime, expiredAt, cancelledTime,
                status, DateTimeOffset.MinValue
            );

            Assert.Equal(
                status,
                entity.Status
            );

            Assert.Equal(
                DateTimeOffset.MinValue,
                entity.CreatedAt
            );

            Assert.Equal(
                DateTimeOffset.MaxValue,
                entity.ExpiresAt
            );

            Assert.Equal(
                confirmationTime,
                entity.ConfirmationTime
            );

            Assert.Equal(
                expiredAt,
                entity.ExpiredAt
            );

            Assert.Equal(
                cancelledTime,
                entity.CancelledTime
            );
        }
        #endregion

        #region Queries
        [Theory]
        [InlineData(ActionStatus.Active)]
        [InlineData(ActionStatus.Confirmed)]
        [InlineData(ActionStatus.Cancelled)]
        [InlineData(ActionStatus.Expired)]
        public void GetExpirationDate_AnyStatus_ReturnsStoredExpiration(ActionStatus status)
        {
            var entity = status switch
            {
                ActionStatus.Confirmed => RestoreConfirmed(T1),
                ActionStatus.Cancelled => RestoreCancelled(T1),
                ActionStatus.Expired => RestoreExpired(D),
                _ => RestoreActive()
            };

            CaptureAndAssertUnchanged(entity, () =>
            {
                Assert.Equal(
                    entity.ExpiresAt,
                    entity.GetExpirationDate()
                );

                Assert.Equal(
                    entity.ExpiresAt.Offset,
                    entity.GetExpirationDate().Offset
                );
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerMinute)]
        [InlineData(-1)]
        public void IsActive_ActiveWithinLifetime_ReturnsTrue(long ticksFromBoundary)
        {
            var entity = RestoreActive();

            var now = ticksFromBoundary switch
            {
                0 => T0,
                1 => T0.AddTicks(1),
                TimeSpan.TicksPerMinute => T1,
                _ => D.AddTicks(-1)
            };

            CaptureAndAssertUnchanged(entity, () =>
                Assert.True(entity.IsActive(now)));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void IsActive_ActiveOutsideLifetime_ReturnsFalse(long ticksFromBoundary)
        {
            var entity = RestoreActive();

            var now = ticksFromBoundary switch
            {
                -1 => T0.AddTicks(-1),
                0 => D,
                _ => D.AddTicks(1)
            };

            CaptureAndAssertUnchanged(entity, () =>
                Assert.False(entity.IsActive(now)));
        }

        [Theory]
        [InlineData(ActionStatus.Confirmed)]
        [InlineData(ActionStatus.Cancelled)]
        [InlineData(ActionStatus.Expired)]
        public void IsActive_TerminalStatus_ReturnsFalse(ActionStatus status)
        {
            var entity = status switch
            {
                ActionStatus.Confirmed => RestoreConfirmed(T1),
                ActionStatus.Cancelled => RestoreCancelled(T1),
                _ => RestoreExpired(D)
            };

            CaptureAndAssertUnchanged(entity, () =>
            {
                Assert.False(entity.IsActive(T0.AddTicks(-1)));
                Assert.False(entity.IsActive(T1));
                Assert.False(entity.IsActive(D));
                Assert.False(entity.IsActive(D.AddTicks(1)));
            });
        }

        [Fact]
        public void IsActive_DifferentOffsets_UsesActualInstant()
        {
            var entity = RestoreActive();

            CaptureAndAssertUnchanged(entity, () =>
            {
                Assert.True(entity.IsActive(T1.ToOffset(TimeSpan.FromHours(2))));
                Assert.False(entity.IsActive(T0.AddTicks(-1).ToOffset(TimeSpan.FromMinutes(15))));
                Assert.False(entity.IsActive(D.ToOffset(TimeSpan.FromMinutes(-15))));
            });
        }
        #endregion

        #region Validate confirmation and cancellation
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_ActiveWithMatchingToken_ReturnsSuccess(bool isConfirmation)
        {
            var entity = RestoreActive();

            var times = new[] { T0, T0.AddTicks(1), T1, D.AddTicks(-1) };

            foreach (var now in times)
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var result = isConfirmation
                        ? entity.ValidateConfirmation(
                            entity.ConfirmationToken,
                            now
                        )
                        : entity.ValidateCancellation(
                            entity.ConfirmationToken,
                            now
                        );

                    Assert.True(result.IsSuccess);
                    Assert.Empty(result.Errors);
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_EqualTokenFromDifferentInstance_ReturnsSuccess(bool isConfirmation)
        {
            var entity = RestoreActive();

            var otherTokenResult = ConfirmationFlowToken.Parse(entity.ConfirmationToken.Value);

            Assert.True(otherTokenResult.IsSuccess);

            var otherToken = otherTokenResult.Value;

            Assert.NotSame(
                entity.ConfirmationToken,
                otherToken
            );

            CaptureAndAssertUnchanged(entity, () =>
            {
                var result = isConfirmation
                    ? entity.ValidateConfirmation(
                        otherToken,
                        T1
                    )
                    : entity.ValidateCancellation(
                        otherToken,
                        T1
                    );

                Assert.True(result.IsSuccess);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_ActiveWithDifferentToken_ReturnsInvalidTokenFailure(bool isConfirmation)
        {
            var entity = RestoreActive(token: CreateToken(0));
            var differentToken = CreateToken(255);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var result = isConfirmation
                    ? entity.ValidateConfirmation(
                        differentToken,
                        T1
                    )
                    : entity.ValidateCancellation(
                        differentToken,
                        T1
                    );

                Assert.True(result.IsFailure);

                var error = Assert.Single(result.Errors);

                Assert.Equal(
                    "PENDING_ACTION.INVALID_CONFIRMATION_TOKEN",
                    error.Code
                );

                Assert.Equal(
                    ErrorType.Forbidden,
                    error.ErrorType
                );

                Assert.Equal(
                    "Pending action confirmation failed for type " +
                    "'UserPendingAction': the provided confirmation token does not match.",
                    error.Message
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_ActiveWithNullToken_ThrowsDataInconsistency(bool isConfirmation)
        {
            var entity = RestoreActive();

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                {
                    if (isConfirmation)
                    {
                        entity.ValidateConfirmation(
                            null!,
                            T1
                        );
                    }
                    else
                    {
                        entity.ValidateCancellation(
                            null!,
                            T1
                        );
                    }
                });

                AssertEmptyField(
                    exception,
                    "confirmationFlowToken",
                    "Update"
                );
            });
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void Validate_ActiveAtOrAfterDeadline_ReturnsExpiredFailure(
            bool isConfirmation,
            long ticksFromDeadline)
        {
            var entity = RestoreActive();

            var now = D.AddTicks(ticksFromDeadline);

            var tokens = new ConfirmationFlowToken?[]
            {
                entity.ConfirmationToken,
                CreateToken(255),
                null
            };

            foreach (var token in tokens)
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var result = isConfirmation
                        ? entity.ValidateConfirmation(
                            token!,
                            now
                        )
                        : entity.ValidateCancellation(
                            token!,
                            now
                        );

                    Assert.True(result.IsFailure);

                    var error = Assert.Single(result.Errors);

                    Assert.Equal(
                        "PENDING_ACTION.EXPIRED_ACTION",
                        error.Code
                    );

                    Assert.Equal(
                        ErrorType.Conflict,
                        error.ErrorType
                    );

                    Assert.Equal(
                        "Pending action processing failed for type " +
                        "'UserPendingAction': the action has expired.",
                        error.Message
                    );
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_NowBeforeCreatedAt_ThrowsDataInconsistency(bool isConfirmation)
        {
            foreach (var status in new[]
                     {
                        ActionStatus.Active,
                        ActionStatus.Confirmed,
                        ActionStatus.Cancelled,
                        ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    ActionStatus.Expired => RestoreExpired(D),
                    _ => RestoreActive()
                };

                var now = T0.AddTicks(-1);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    {
                        if (isConfirmation)
                        {
                            entity.ValidateConfirmation(
                                entity.ConfirmationToken,
                                now
                            );
                        }
                        else
                        {
                            entity.ValidateCancellation(
                                entity.ConfirmationToken,
                                now
                            );
                        }
                    });

                    AssertOutOfRangeNow(
                        exception,
                        T0
                    );
                });

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    {
                        if (isConfirmation)
                        {
                            entity.ValidateConfirmation(
                                null!,
                                now
                            );
                        }
                        else
                        {
                            entity.ValidateCancellation(
                                null!,
                                now
                            );
                        }
                    });

                    AssertOutOfRangeNow(
                        exception,
                        T0
                    );
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_TerminalStatus_ThrowsInvariantViolation(bool isConfirmation)
        {
            foreach (var status in new[]
             {
                ActionStatus.Confirmed,
                ActionStatus.Cancelled,
                ActionStatus.Expired
             })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    _ => RestoreExpired(D)
                };

                foreach (var now in new[] { T1, D, D.AddTicks(1) })
                {
                    foreach (var useNullToken in new[] { false, true })
                    {
                        CaptureAndAssertUnchanged(entity, () =>
                        {
                            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                            {
                                var token = useNullToken ? null! : entity.ConfirmationToken;

                                if (isConfirmation)
                                {
                                    entity.ValidateConfirmation(
                                        token,
                                        now
                                    );
                                }
                                else
                                {
                                    entity.ValidateCancellation(
                                        token,
                                        now
                                    );
                                }
                            });

                            AssertTerminalInvariant(
                                exception,
                                entity.Id.Value,
                                entity.UserId.Value,
                                status,
                                entity.ExpiresAt,
                                now
                            );
                        });
                    }
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_DeadlineWithDifferentOffset_ReturnsExpiredFailure(bool isConfirmation)
        {
            var entity = RestoreActive();
            var now = D.ToOffset(TimeSpan.FromMinutes(-15));

            CaptureAndAssertUnchanged(entity, () =>
            {
                var result = isConfirmation
                    ? entity.ValidateConfirmation(
                        entity.ConfirmationToken,
                        now
                    )
                    : entity.ValidateCancellation(
                        entity.ConfirmationToken,
                        now
                    );

                Assert.True(result.IsFailure);

                Assert.Equal(
                    "PENDING_ACTION.EXPIRED_ACTION",
                    Assert.Single(result.Errors).Code
                );
            });
        }
        #endregion

        #region Mark as confirmed or cancelled
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_ActiveWithMatchingToken_ChangesExpectedState(bool isConfirm)
        {
            var times = new[]
            {
                T0,
                T0.AddTicks(1),
                T1,
                D.AddTicks(-1),
                T1.ToOffset(TimeSpan.FromHours(2))
            };

            foreach (var now in times)
            {
                var entity = RestoreActive();

                if (isConfirm)
                {
                    entity.MarkAsConfirmed(
                        entity.ConfirmationToken,
                        now
                    );
                }
                else
                {
                    entity.MarkAsCancelled(
                        entity.ConfirmationToken,
                        now
                    );
                }

                Assert.Equal(
                    isConfirm ? ActionStatus.Confirmed : ActionStatus.Cancelled,
                    entity.Status
                );

                if (isConfirm)
                {
                    Assert.Equal(
                        now,
                        entity.ConfirmationTime
                    );

                    Assert.Equal(
                        now.Offset,
                        entity.ConfirmationTime!.Value.Offset
                    );

                    Assert.Null(entity.CancelledTime);
                }
                else
                {
                    Assert.Equal(
                        now,
                        entity.CancelledTime
                    );

                    Assert.Equal(
                        now.Offset,
                        entity.CancelledTime!.Value.Offset
                    );

                    Assert.Null(entity.ConfirmationTime);
                }

                Assert.Null(entity.ExpiredAt);
                Assert.Empty(entity.DomainEvents);
                Assert.Empty(entity.AggregateChanges);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_EqualTokenFromDifferentInstance_ChangesExpectedState(bool isConfirm)
        {
            var entity = RestoreActive();

            var other = ConfirmationFlowToken.Parse(entity.ConfirmationToken.Value);

            Assert.True(other.IsSuccess);

            Assert.NotSame(
                entity.ConfirmationToken,
                other.Value
            );

            if (isConfirm)
            {
                entity.MarkAsConfirmed(
                    other.Value,
                    T1
                );
            }
            else
            {
                entity.MarkAsCancelled(
                    other.Value,
                    T1
                );
            }

            Assert.Equal(
                isConfirm ? ActionStatus.Confirmed : ActionStatus.Cancelled,
                entity.Status
            );

            if (isConfirm)
            {
                Assert.Equal(
                    T1,
                    entity.ConfirmationTime
                );

                Assert.Null(entity.CancelledTime);
            }
            else
            {
                Assert.Equal(
                    T1,
                    entity.CancelledTime
                );

                Assert.Null(entity.ConfirmationTime);
            }

            Assert.Null(entity.ExpiredAt);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_DifferentToken_ThrowsPreconditionFailed(bool isConfirm)
        {
            var entity = RestoreActive(token: CreateToken(0));

            var different = CreateToken(255);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                {
                    if (isConfirm)
                    {
                        entity.MarkAsConfirmed(
                            different,
                            T1
                        );
                    }
                    else
                    {
                        entity.MarkAsCancelled(
                            different,
                            T1
                        );
                    }
                });

                AssertMarkPreconditionFailed(
                    exception,
                    isConfirm,
                    entity,
                    T1,
                    "PENDING_ACTION.INVALID_CONFIRMATION_TOKEN"
                );
            });
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void Mark_AtOrAfterDeadline_ThrowsPreconditionFailed(
            bool isConfirm,
            long ticksFromDeadline)
        {
            var entity = RestoreActive(token: CreateToken(0));

            var now = D.AddTicks(ticksFromDeadline);

            var tokens = new ConfirmationFlowToken?[]
            {
                entity.ConfirmationToken,
                CreateToken(255),
                null
            };

            foreach (var token in tokens)
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                    {
                        if (isConfirm)
                        {
                            entity.MarkAsConfirmed(
                                token!,
                                now
                            );
                        }
                        else
                        {
                            entity.MarkAsCancelled(
                                token!,
                                now
                            );
                        }
                    });

                    AssertMarkPreconditionFailed(
                        exception,
                        isConfirm,
                        entity,
                        now,
                        "PENDING_ACTION.EXPIRED_ACTION"
                    );
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_NullTokenInsideLifetime_ThrowsDataInconsistency(bool isConfirm)
        {
            var entity = RestoreActive();

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                {
                    if (isConfirm)
                    {
                        entity.MarkAsConfirmed(
                            null!,
                            T1
                        );
                    }
                    else
                    {
                        entity.MarkAsCancelled(
                            null!,
                            T1
                        );
                    }

                });

                AssertEmptyField(
                    exception,
                    "confirmationFlowToken",
                    "Update"
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_NowBeforeCreatedAt_ThrowsDataInconsistency(bool isConfirm)
        {
            foreach (var status in new[]
                     {
                        ActionStatus.Active,
                        ActionStatus.Confirmed,
                        ActionStatus.Cancelled,
                        ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    ActionStatus.Expired => RestoreExpired(D),
                    _ => RestoreActive()
                };
                var now = T0.AddTicks(-1);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    {
                        if (isConfirm)
                        {
                            entity.MarkAsConfirmed(
                                entity.ConfirmationToken,
                                now
                            );
                        }
                        else
                        {
                            entity.MarkAsCancelled(
                                entity.ConfirmationToken,
                                now
                            );
                        }
                    });

                    AssertOutOfRangeNow(
                        exception,
                        T0
                    );
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_TerminalStatus_ThrowsInvariantViolation(bool isConfirm)
        {
            foreach (var status in new[]
                     {
                        ActionStatus.Confirmed,
                        ActionStatus.Cancelled,
                        ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    _ => RestoreExpired(D)
                };

                foreach (var now in new[] { T1, D, D.AddTicks(1) })
                {
                    CaptureAndAssertUnchanged(entity, () =>
                    {
                        var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                        {
                            if (isConfirm)
                            {
                                entity.MarkAsConfirmed(
                                    entity.ConfirmationToken,
                                    now
                                );
                            }
                            else
                            {
                                entity.MarkAsCancelled(
                                    entity.ConfirmationToken,
                                    now
                                );
                            }
                        });

                        AssertTerminalInvariant(
                            exception,
                            entity.Id.Value,
                            entity.UserId.Value,
                            status,
                            entity.ExpiresAt,
                            now
                        );
                    });
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Mark_RepeatedCall_ThrowsInvariantAndPreservesFirstTransition(bool isConfirm)
        {
            var entity = RestoreActive();

            if (isConfirm)
            {
                entity.MarkAsConfirmed(
                    entity.ConfirmationToken,
                    T1
                );
            }
            else
            {
                entity.MarkAsCancelled(
                    entity.ConfirmationToken,
                    T1
                );
            }

            var firstConfirmation = entity.ConfirmationTime;
            var firstCancelled = entity.CancelledTime;

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (isConfirm)
                    {
                        entity.MarkAsConfirmed(
                            entity.ConfirmationToken,
                            T1.AddMinutes(1)
                        );
                    }
                    else
                    {
                        entity.MarkAsCancelled(
                            entity.ConfirmationToken,
                            T1.AddMinutes(1)
                        );
                    }
                });

                Assert.Equal(
                    "Only an active pending action can transition to another state.",
                    exception.Context["Rule"]
                );
            });

            Assert.Equal(
                firstConfirmation,
                entity.ConfirmationTime
            );

            Assert.Equal(
                firstCancelled,
                entity.CancelledTime
            );
        }
        #endregion

        #region ValidateCanExpire
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateCanExpire_ActiveBeforeDeadline_ThrowsPreconditionFailed(long ticksFromDeadline)
        {
            var entity = RestoreActive();

            var now = ticksFromDeadline == 0 ? T0 : D.AddTicks(-1);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                    entity.ValidateCanExpire(now));

                AssertExpirePreconditionFailed(
                    exception,
                    entity,
                    now
                );
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateCanExpire_ActiveAtOrAfterDeadline_DoesNotThrow(long ticksFromDeadline)
        {
            var entity = RestoreActive();
            var now = D.AddTicks(ticksFromDeadline);

            CaptureAndAssertUnchanged(entity, () =>
                entity.ValidateCanExpire(now));
        }

        [Fact]
        public void ValidateCanExpire_DeadlineWithDifferentOffset_DoesNotThrow()
        {
            var entity = RestoreActive();
            var now = D.ToOffset(TimeSpan.FromMinutes(-15));

            CaptureAndAssertUnchanged(entity, () =>
                entity.ValidateCanExpire(now));
        }

        [Fact]
        public void ValidateCanExpire_ActiveAtMaxValue_DoesNotThrow()
        {
            var entity = RestoreActive();

            CaptureAndAssertUnchanged(entity, () =>
                entity.ValidateCanExpire(DateTimeOffset.MaxValue));
        }

        [Fact]
        public void ValidateCanExpire_NowBeforeCreatedAt_ThrowsDataInconsistency()
        {
            foreach (var status in new[]
                     {
                        ActionStatus.Active,
                        ActionStatus.Confirmed,
                        ActionStatus.Cancelled,
                        ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    ActionStatus.Expired => RestoreExpired(D),
                    _ => RestoreActive()
                };

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                        entity.ValidateCanExpire(T0.AddTicks(-1)));

                    AssertOutOfRangeNow(
                        exception,
                        T0
                    );
                });
            }
        }

        [Fact]
        public void ValidateCanExpire_TerminalStatus_ThrowsInvariantViolation()
        {
            foreach (var status in new[]
                     {
                        ActionStatus.Confirmed,
                        ActionStatus.Cancelled,
                        ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    _ => RestoreExpired(D)
                };

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                        entity.ValidateCanExpire(D));

                    AssertTerminalInvariant(
                        exception,
                        entity.Id.Value,
                        entity.UserId.Value,
                        status,
                        entity.ExpiresAt,
                        D
                    );
                });
            }
        }
        #endregion

        #region MarkAsExpired
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void MarkAsExpired_ActiveAtOrAfterDeadline_Expires(long ticksFromDeadline)
        {
            var entity = RestoreActive();
            var now = ticksFromDeadline == 0 ? D : D.AddTicks(1);

            entity.MarkAsExpired(now);

            Assert.Equal(
                ActionStatus.Expired,
                entity.Status
            );

            Assert.Equal(
                now,
                entity.ExpiredAt
            );

            Assert.Null(entity.ConfirmationTime);
            Assert.Null(entity.CancelledTime);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Fact]
        public void MarkAsExpired_ActiveAtMaxValue_Expires()
        {
            var entity = RestoreActive();

            entity.MarkAsExpired(DateTimeOffset.MaxValue);

            Assert.Equal(
                ActionStatus.Expired,
                entity.Status
            );

            Assert.Equal(
                DateTimeOffset.MaxValue,
                entity.ExpiredAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MarkAsExpired_ActiveBeforeDeadline_ThrowsPreconditionFailed(long ticksFromDeadline)
        {
            var entity = RestoreActive();
            var now = ticksFromDeadline == 0 ? T0 : D.AddTicks(-1);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                    entity.MarkAsExpired(now));

                AssertExpirePreconditionFailed(
                    exception,
                    entity,
                    now
                );
            });
        }

        [Fact]
        public void MarkAsExpired_NowBeforeCreatedAt_ThrowsDataInconsistency()
        {
            foreach (var status in new[]
                     {
                         ActionStatus.Active, ActionStatus.Confirmed,
                         ActionStatus.Cancelled, ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    ActionStatus.Expired => RestoreExpired(D),
                    _ => RestoreActive()
                };

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                        entity.MarkAsExpired(T0.AddTicks(-1)));

                    AssertOutOfRangeNow(
                        exception,
                        T0
                    );
                });
            }
        }

        [Fact]
        public void MarkAsExpired_TerminalStatus_ThrowsInvariantViolation()
        {
            foreach (var status in new[]
                     {
                        ActionStatus.Confirmed,
                        ActionStatus.Cancelled,
                        ActionStatus.Expired
                     })
            {
                var entity = status switch
                {
                    ActionStatus.Confirmed => RestoreConfirmed(T1),
                    ActionStatus.Cancelled => RestoreCancelled(T1),
                    _ => RestoreExpired(D)
                };

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                        entity.MarkAsExpired(D.AddHours(1)));

                    AssertTerminalInvariant(
                        exception,
                        entity.Id.Value,
                        entity.UserId.Value,
                        status,
                        entity.ExpiresAt,
                        D.AddHours(1)
                    );
                });
            }
        }

        [Fact]
        public void MarkAsExpired_RepeatedCall_ThrowsInvariantAndPreservesExpiredAt()
        {
            var entity = RestoreActive();
            entity.MarkAsExpired(D);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    entity.MarkAsExpired(D.AddHours(1)));

                Assert.Equal(
                    "Only an active pending action can transition to another state.",
                    exception.Context["Rule"]
                );
            });

            Assert.Equal(
                D,
                entity.ExpiredAt
            );
        }

        [Fact]
        public void MarkAsExpired_DeadlineWithDifferentOffset_Expires()
        {
            var entity = RestoreActive();
            var now = D.ToOffset(TimeSpan.FromHours(2));

            entity.MarkAsExpired(now);

            Assert.Equal(
                ActionStatus.Expired,
                entity.Status
            );

            Assert.Equal(
                now,
                entity.ExpiredAt
            );

            Assert.Equal(
                now.Offset,
                entity.ExpiredAt!.Value.Offset
            );
        }
        #endregion

        #region Restore: domain operation round trip
        // Відновлюємо стани, отримані реальними доменними переходами, а не лише фабриками тестів.
        // Restore states produced by actual domain transitions as well as test factories.
        [Fact]
        public void Restore_StateProducedByDomainOperations_ReconstructsSameState()
        {
            var created = CreateActive();

            var restoredFromCreated = UserPendingAction.Restore(
                created.Id, created.UserId, created.ConfirmationToken,
                created.UserAction, created.ExpiresAt,
                created.ConfirmationTime, created.ExpiredAt, created.CancelledTime,
                created.Status, created.CreatedAt
            );

            Assert.NotSame(
                created,
                restoredFromCreated
            );

            AssertUnchanged(
                restoredFromCreated,
                created.Id, created.UserId, created.ConfirmationToken, created.UserAction,
                created.CreatedAt, created.ExpiresAt, created.Status,
                created.ConfirmationTime, created.ExpiredAt, created.CancelledTime
            );

            var confirmed = CreateActive();

            confirmed.MarkAsConfirmed(confirmed.ConfirmationToken, T1);

            var restoredConfirmed = UserPendingAction.Restore(
                confirmed.Id, confirmed.UserId, confirmed.ConfirmationToken,
                confirmed.UserAction, confirmed.ExpiresAt,
                confirmed.ConfirmationTime, confirmed.ExpiredAt, confirmed.CancelledTime,
                confirmed.Status, confirmed.CreatedAt
            );

            Assert.NotSame(
                confirmed,
                restoredConfirmed
            );

            AssertUnchanged(
                restoredConfirmed,
                confirmed.Id, confirmed.UserId, confirmed.ConfirmationToken,
                confirmed.UserAction, confirmed.CreatedAt, confirmed.ExpiresAt,
                confirmed.Status, confirmed.ConfirmationTime, confirmed.ExpiredAt,
                confirmed.CancelledTime
            );

            var cancelled = CreateActive();

            cancelled.MarkAsCancelled(cancelled.ConfirmationToken, T1);

            var restoredCancelled = UserPendingAction.Restore(
                cancelled.Id, cancelled.UserId, cancelled.ConfirmationToken,
                cancelled.UserAction, cancelled.ExpiresAt,
                cancelled.ConfirmationTime, cancelled.ExpiredAt, cancelled.CancelledTime,
                cancelled.Status, cancelled.CreatedAt
            );

            Assert.NotSame(
                cancelled,
                restoredCancelled
            );

            AssertUnchanged(
                restoredCancelled,
                cancelled.Id, cancelled.UserId, cancelled.ConfirmationToken,
                cancelled.UserAction, cancelled.CreatedAt, cancelled.ExpiresAt,
                cancelled.Status, cancelled.ConfirmationTime, cancelled.ExpiredAt,
                cancelled.CancelledTime
            );

            var expired = CreateActive();

            expired.MarkAsExpired(D);

            var restoredExpired = UserPendingAction.Restore(
                expired.Id, expired.UserId, expired.ConfirmationToken,
                expired.UserAction, expired.ExpiresAt,
                expired.ConfirmationTime, expired.ExpiredAt, expired.CancelledTime,
                expired.Status, expired.CreatedAt
            );

            Assert.NotSame(
                expired,
                restoredExpired
            );

            AssertUnchanged(
                restoredExpired,
                expired.Id, expired.UserId, expired.ConfirmationToken,
                expired.UserAction, expired.CreatedAt, expired.ExpiresAt,
                expired.Status, expired.ConfirmationTime, expired.ExpiredAt,
                expired.CancelledTime
            );
        }
        #endregion
    }
}
