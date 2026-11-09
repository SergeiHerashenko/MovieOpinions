using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersDeletion
{
    public class UserDeletionTests
    {
        private static readonly DateTimeOffset T0 = new(
            2026, 11, 5,
            12, 0, 0,
            TimeSpan.Zero
        );

        private static readonly DateTimeOffset D = T0.AddDays(30);
        private static readonly DateTimeOffset T1 = T0.AddDays(1);

        private static UserId CreateUserId()
            => UserId.Restore(Guid.Parse("11111111-1111-4111-8111-111111111111"));

        private static UserDeletionId CreateDeletionId()
            => UserDeletionId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222")
            );

        private static Login CreateEmailLogin()
            => Login.From(Email.Restore("serhii@example.com"));

        private static Login CreatePhoneLogin()
            => Login.From(
                Phone.Restore(
                    PhoneCountryCode.Restore("+380"),
                    PhoneNationalNumber.Restore("981234567")
                )
            );

        private static DeletionReason CreateReason(string text = "Account closed")
        {
            var result = DeletionReason.Create(text);

            Assert.True(result.IsSuccess);

            return result.Value;
        }

        private static UserDeletion CreateDeleted(
            Login? login = null,
            DateTimeOffset? now = null)
            => UserDeletion.Create(
                CreateUserId(),
                login ?? CreateEmailLogin(),
                now ?? T0,
                CreateReason()
            );

        private static UserDeletion RestoreDeleted(
            DateTimeOffset? createdAt = null,
            DateTimeOffset? restoreUntil = null)
            => UserDeletion.Restore(
                CreateDeletionId(),
                CreateUserId(),
                CreateEmailLogin(),
                CreateReason(),
                createdAt ?? T0,
                restoreUntil ?? (createdAt ?? T0).AddDays(30),
                null,
                DeletionStatus.Deleted,
                null
            );

        private static UserDeletion RestoreRestored(
            DateTimeOffset restoredAt,
            DateTimeOffset? updatedAt = null)
            => UserDeletion.Restore(
                CreateDeletionId(),
                CreateUserId(),
                CreateEmailLogin(),
                CreateReason(),
                T0,
                D,
                restoredAt,
                DeletionStatus.Restored,
                updatedAt ?? restoredAt
            );

        private static UserDeletion RestorePermanentlyDeleted(DateTimeOffset updatedAt)
            => UserDeletion.Restore(
                CreateDeletionId(),
                CreateUserId(),
                CreateEmailLogin(),
                CreateReason(),
                T0,
                D,
                null,
                DeletionStatus.PermanentlyDeleted,
                updatedAt
            );

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
                "UserDeletion",
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

        private static void AssertInvariant(
            DomainInvariantViolationException exception,
            string rule)
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
                "UserDeletion",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                rule,
                exception.Context["Rule"]
            );
        }

        private static void AssertInconsistentState(
            DomainInvariantViolationException exception,
            DeletionStatus expectedStatus,
            object? expectedUpdatedAt,
            object? expectedRestoredAt,
            DateTimeOffset expectedCreatedAt,
            DateTimeOffset expectedRestoreUntil)
        {
            AssertInvariant(
                exception,
                "Inconsistent state: fields 'deletionStatus' and " +
                "'updatedAt' and 'restoredAt' are not consistent!"
            );

            Assert.Equal(
                expectedStatus,
                exception.Context["State_DeletionStatus"]
            );

            Assert.Equal(
                expectedUpdatedAt ?? "null",
                exception.Context["State_UpdatedAt"]
            );

            Assert.Equal(
                expectedRestoredAt ?? "null",
                exception.Context["State_RestoredAt"]
            );

            Assert.Equal(
                expectedCreatedAt,
                exception.Context["State_CreatedAt"]
            );

            Assert.Equal(
                expectedRestoreUntil,
                exception.Context["State_RestoreUntil"]
            );
        }

        private static void AssertUnchanged(
            UserDeletion entity,
            UserDeletionId expectedId,
            UserId expectedUserId,
            Login expectedLogin,
            DeletionReason expectedReason,
            DateTimeOffset expectedCreatedAt,
            DateTimeOffset expectedRestoreUntil,
            DateTimeOffset? expectedRestoredAt,
            DeletionStatus expectedStatus,
            DateTimeOffset? expectedUpdatedAt)
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
                expectedLogin,
                entity.Login
            );

            Assert.Same(
                expectedReason,
                entity.Reason
            );

            Assert.Equal(
                expectedCreatedAt,
                entity.CreatedAt
            );

            Assert.Equal(
                expectedRestoreUntil,
                entity.RestoreUntil
            );

            Assert.Equal(
                expectedRestoredAt,
                entity.RestoredAt
            );

            Assert.Equal(
                expectedStatus,
                entity.Status
            );

            Assert.Equal(
                expectedUpdatedAt,
                entity.UpdatedAt
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ValidArguments_SetsInitialState(bool useEmailLogin)
        {
            var userId = CreateUserId();

            var login = useEmailLogin ? CreateEmailLogin() : CreatePhoneLogin();
            var reason = CreateReason();

            var now = new DateTimeOffset(
                2026, 11, 5,
                12, 0, 0,
                TimeSpan.FromHours(2)
            );

            var entity = UserDeletion.Create(
                userId,
                login,
                now,
                reason
            );

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
                login,
                entity.Login
            );

            Assert.Same(
                reason,
                entity.Reason
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
                now.AddDays(30),
                entity.RestoreUntil
            );

            Assert.Equal(
                now.Offset,
                entity.RestoreUntil.Offset
            );

            Assert.Equal(
                DeletionStatus.Deleted,
                entity.Status
            );

            Assert.Null(entity.RestoredAt);
            Assert.Null(entity.UpdatedAt);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Fact]
        public void Create_SeparateCalls_ReturnDifferentIds()
        {
            var first = CreateDeleted();
            var second = CreateDeleted();

            Assert.NotEqual(
                first.Id.Value,
                second.Id.Value
            );
        }

        [Theory]
        [InlineData("userId")]
        [InlineData("login")]
        [InlineData("reason")]
        public void Create_NullRequiredArgument_ThrowsDomainDataInconsistencyException(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserDeletion.Create(
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "login" ? null! : CreateEmailLogin(),
                    T0,
                    nullField == "reason" ? null! : CreateReason()
                )
            );

            AssertEmptyField(
                exception,
                nullField,
                "Create"
            );
        }

        [Theory]
        [InlineData(true, true, true, "userId")]
        [InlineData(false, true, true, "login")]
        public void Create_MultipleNullArguments_ReportsFirstNullField(
            bool nullUserId,
            bool nullLogin,
            bool nullReason,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserDeletion.Create(
                    nullUserId ? null! : CreateUserId(),
                    nullLogin ? null! : CreateEmailLogin(),
                    T0,
                    nullReason ? null! : CreateReason()
                )
            );

            AssertEmptyField(
                exception,
                expectedField,
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
                : DateTimeOffset.MaxValue.AddDays(-30);

            var entity = UserDeletion.Create(
                CreateUserId(),
                CreateEmailLogin(),
                now,
                CreateReason()
            );

            Assert.Equal(
                now,
                entity.CreatedAt
            );

            Assert.Equal(
                now.AddDays(30),
                entity.RestoreUntil
            );

            if (!useMinValue)
            {
                Assert.Equal(
                    DateTimeOffset.MaxValue,
                    entity.RestoreUntil
                );
            }
        }

        [Fact]
        public void Create_RestorationDeadlineOverflows_ThrowsArgumentOutOfRangeException()
        {
            var now = DateTimeOffset.MaxValue.AddDays(-30).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UserDeletion.Create(
                    CreateUserId(),
                    CreateEmailLogin(),
                    now,
                    CreateReason()
                )
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay)]
        [InlineData(60 * TimeSpan.TicksPerDay)]
        public void Restore_DeletedState_PreservesStoredValues(long durationTicks)
        {
            var id = CreateDeletionId();
            var userId = CreateUserId();
            var login = CreateEmailLogin();
            var reason = CreateReason();
            var restoreUntil = T0.AddTicks(durationTicks);

            var entity = UserDeletion.Restore(
                id,
                userId,
                login,
                reason,
                T0,
                restoreUntil,
                null,
                DeletionStatus.Deleted,
                null
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
                login,
                entity.Login
            );

            Assert.Same(
                reason,
                entity.Reason
            );

            Assert.Equal(
                T0,
                entity.CreatedAt
            );

            Assert.Equal(
                restoreUntil,
                entity.RestoreUntil
            );

            Assert.Null(entity.RestoredAt);

            Assert.Equal(
                DeletionStatus.Deleted,
                entity.Status
            );

            Assert.Null(entity.UpdatedAt);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay - 1)]
        public void Restore_RestoredStateWithValidTimestamps_Succeeds(long ticksFromCreated)
        {
            var restoredAt = T0.AddTicks(ticksFromCreated);

            var entity = RestoreRestored(restoredAt);

            Assert.Equal(
                DeletionStatus.Restored,
                entity.Status
            );

            Assert.Equal(
                restoredAt,
                entity.RestoredAt
            );

            Assert.Equal(
                restoredAt,
                entity.UpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void Restore_PermanentlyDeletedStateWithValidTimestamps_Succeeds(long ticksFromDeadline)
        {
            var updatedAt = D.AddTicks(ticksFromDeadline);

            var entity = RestorePermanentlyDeleted(updatedAt);

            Assert.Equal(
                DeletionStatus.PermanentlyDeleted,
                entity.Status
            );

            Assert.Equal(
                updatedAt,
                entity.UpdatedAt
            );

            Assert.Null(entity.RestoredAt);
        }

        [Theory]
        [InlineData("userDeletionId")]
        [InlineData("userId")]
        [InlineData("login")]
        [InlineData("reason")]
        public void Restore_NullRequiredArgument_ThrowsDomainDataInconsistencyException(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserDeletion.Restore(
                    nullField == "userDeletionId" ? null! : CreateDeletionId(),
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "login" ? null! : CreateEmailLogin(),
                    nullField == "reason" ? null! : CreateReason(),
                    T0,
                    D,
                    null,
                    DeletionStatus.Deleted,
                    null
                )
            );

            AssertEmptyField(
                exception,
                nullField,
                "Restore"
            );
        }

        [Theory]
        [InlineData(true, true, true, true, "userDeletionId")]
        [InlineData(false, true, true, true, "userId")]
        [InlineData(false, false, true, true, "login")]
        public void Restore_MultipleNullArguments_ReportsFirstNullField(
            bool nullDeletionId,
            bool nullUserId,
            bool nullLogin,
            bool nullReason,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserDeletion.Restore(
                    nullDeletionId ? null! : CreateDeletionId(),
                    nullUserId ? null! : CreateUserId(),
                    nullLogin ? null! : CreateEmailLogin(),
                    nullReason ? null! : CreateReason(),
                    T0,
                    D,
                    null,
                    DeletionStatus.Deleted,
                    null
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
        public void Restore_UndefinedDeletionStatus_ThrowsDomainDataInconsistencyException(int statusValue)
        {
            var status = (DeletionStatus)statusValue;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    null,
                    status,
                    null
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "deletionStatus",
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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Restore_RestoreUntilNotLaterThanCreatedAt_ThrowsDomainInvariantViolationException(long ticksOffset)
        {
            var restoreUntil = T0.AddTicks(ticksOffset);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    restoreUntil,
                    null,
                    DeletionStatus.Deleted,
                    null
                )
            );

            AssertInvariant(
                exception,
                "RestoreUntil must be later than CreatedAt."
            );

            Assert.Equal(
                T0,
                exception.Context["State_CreatedAt"]
            );

            Assert.Equal(
                restoreUntil,
                exception.Context["State_RestoreUntil"]
            );
        }

        [Fact]
        public void Restore_UpdatedAtBeforeCreatedAt_ThrowsDomainInvariantViolationException()
        {
            var updatedAt = T0.AddTicks(-1);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    T0,
                    DeletionStatus.Restored,
                    updatedAt
                )
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                T0,
                exception.Context["State_CreatedAt"]
            );

            Assert.Equal(
                updatedAt,
                exception.Context["State_UpdatedAt"]
            );

            AssertInvariant(
                exception,
                $"Update time '{updatedAt}' cannot be less than creation time '{T0}'!"
            );
        }

        [Fact]
        public void Restore_RestoredAtBeforeCreatedAt_ThrowsDomainInvariantViolationException()
        {
            var restoredAt = T0.AddTicks(-1);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    restoredAt,
                    DeletionStatus.Restored,
                    T0
                )
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                restoredAt,
                exception.Context["State_RestoredAt"]
            );

            Assert.Equal(
                T0,
                exception.Context["State_CreatedAt"]
            );

            AssertInvariant(
                exception,
                $"The recovery time '{restoredAt}' cannot be less than creation time '{T0}'!"
            );
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Restore_DeletedStateWithTransitionTimestamps_ThrowsDomainInvariantViolationException(
            bool hasUpdatedAt,
            bool hasRestoredAt)
        {
            DateTimeOffset? updatedAt = hasUpdatedAt ? T1 : null;
            DateTimeOffset? restoredAt = hasRestoredAt ? T1 : null;

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    restoredAt,
                    DeletionStatus.Deleted,
                    updatedAt
                )
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.Deleted,
                updatedAt,
                restoredAt,
                T0,
                D
            );
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Restore_RestoredStateWithMissingTimestamps_ThrowsDomainInvariantViolationException(
            bool hasUpdatedAt,
            bool hasRestoredAt)
        {
            DateTimeOffset? updatedAt = hasUpdatedAt ? T1 : null;
            DateTimeOffset? restoredAt = hasRestoredAt ? T1 : null;

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    restoredAt,
                    DeletionStatus.Restored,
                    updatedAt
                )
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.Restored,
                updatedAt,
                restoredAt,
                T0,
                D
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Restore_RestoredAtAtOrAfterDeadline_ThrowsDomainInvariantViolationException(long ticksFromDeadline)
        {
            var restoredAt = D.AddTicks(ticksFromDeadline);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestoreRestored(restoredAt)
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.Restored,
                restoredAt,
                restoredAt,
                T0,
                D
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public void Restore_RestoredStateWithDifferentUpdateAndRestoreTimes_ThrowsDomainInvariantViolationException(long ticksOffset)
        {
            var restoredAt = T1;
            var updatedAt = T1.AddTicks(ticksOffset);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    restoredAt,
                    DeletionStatus.Restored,
                    updatedAt
                )
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.Restored,
                updatedAt,
                restoredAt,
                T0,
                D
            );
        }

        [Fact]
        public void Restore_PermanentlyDeletedStateWithoutUpdatedAt_ThrowsDomainInvariantViolationException()
        {
            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    null,
                    DeletionStatus.PermanentlyDeleted,
                    null
                )
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.PermanentlyDeleted,
                null,
                null,
                T0,
                D
            );
        }

        [Fact]
        public void Restore_PermanentlyDeletedStateWithRestoredAt_ThrowsDomainInvariantViolationException()
        {
            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserDeletion.Restore(
                    CreateDeletionId(),
                    CreateUserId(),
                    CreateEmailLogin(),
                    CreateReason(),
                    T0,
                    D,
                    T1,
                    DeletionStatus.PermanentlyDeleted,
                    D
                )
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.PermanentlyDeleted,
                D,
                T1,
                T0,
                D
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Restore_PermanentlyDeletedStateUpdatedBeforeDeadline_ThrowsDomainInvariantViolationException(long ticksFromCreatedOrDeadline)
        {
            var updatedAt = ticksFromCreatedOrDeadline == 0
                ? T0
                : D.AddTicks(-1);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestorePermanentlyDeleted(updatedAt)
            );

            AssertInconsistentState(
                exception,
                DeletionStatus.PermanentlyDeleted,
                updatedAt,
                null,
                T0,
                D
            );
        }

        [Fact]
        public void Restore_SameTransitionInstantWithDifferentOffsets_Succeeds()
        {
            var restoredAt = T1.ToOffset(TimeSpan.FromHours(2));
            var updatedAt = T1.ToOffset(TimeSpan.FromHours(-3));

            var entity = UserDeletion.Restore(
                CreateDeletionId(),
                CreateUserId(),
                CreateEmailLogin(),
                CreateReason(),
                T0,
                D,
                restoredAt,
                DeletionStatus.Restored,
                updatedAt
            );

            Assert.Equal(
                restoredAt,
                entity.RestoredAt
            );

            Assert.Equal(
                restoredAt.Offset,
                entity.RestoredAt!.Value.Offset
            );

            Assert.Equal(
                updatedAt,
                entity.UpdatedAt
            );

            Assert.Equal(
                updatedAt.Offset,
                entity.UpdatedAt!.Value.Offset
            );
        }

        [Theory]
        [InlineData(DeletionStatus.Deleted)]
        [InlineData(DeletionStatus.Restored)]
        [InlineData(DeletionStatus.PermanentlyDeleted)]
        public void Restore_ExtremeValidTimestamps_PreservesValues(DeletionStatus status)
        {
            DateTimeOffset? restoredAt = status == DeletionStatus.Restored
                ? DateTimeOffset.MinValue
                : null;

            DateTimeOffset? updatedAt = status switch
            {
                DeletionStatus.Deleted => null,
                DeletionStatus.Restored => DateTimeOffset.MinValue,
                _ => DateTimeOffset.MaxValue
            };

            var entity = UserDeletion.Restore(
                CreateDeletionId(),
                CreateUserId(),
                CreateEmailLogin(),
                CreateReason(),
                DateTimeOffset.MinValue,
                DateTimeOffset.MaxValue,
                restoredAt,
                status,
                updatedAt
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
                entity.RestoreUntil
            );

            Assert.Equal(
                restoredAt,
                entity.RestoredAt
            );

            Assert.Equal(
                updatedAt,
                entity.UpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay - 1)]
        public void Undelete_DeletedBeforeDeadline_RestoresAccount(long ticksFromCreated)
        {
            var entity = RestoreDeleted();
            var now = T0.AddTicks(ticksFromCreated);

            var result = entity.Undelete(now);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                DeletionStatus.Restored,
                entity.Status
            );

            Assert.Equal(
                now,
                entity.RestoredAt
            );

            Assert.Equal(
                now,
                entity.UpdatedAt
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(DeletionStatus.Deleted)]
        [InlineData(DeletionStatus.Restored)]
        [InlineData(DeletionStatus.PermanentlyDeleted)]
        public void Undelete_NowBeforeCreatedAt_ThrowsWithoutChangingState(DeletionStatus status)
        {
            var entity = status switch
            {
                DeletionStatus.Deleted => RestoreDeleted(),
                DeletionStatus.Restored => RestoreRestored(T1),
                _ => RestorePermanentlyDeleted(D)
            };

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                entity.Undelete(T0.AddTicks(-1))
            );

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
                "UserDeletion",
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
                T0,
                exception.Context["Custom_MinimumValue"]
            );

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Undelete_DeletedAtOrAfterDeadline_ReturnsExpiredFailureWithoutChangingState(long ticksFromDeadline)
        {
            var entity = RestoreDeleted();

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var result = entity.Undelete(D.AddTicks(ticksFromDeadline));

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                "USER_DELETION.RESTORATION_PERIOD_EXPIRED",
                error.Code
            );

            Assert.Equal(
                ErrorType.Forbidden,
                error.ErrorType
            );

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay + 1)]
        public void Undelete_AlreadyRestored_ReturnsConflictWithoutChangingState(long ticksFromCreated)
        {
            var entity = RestoreRestored(T1);

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var result = entity.Undelete(T0.AddTicks(ticksFromCreated));

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                "USER_DELETION.USER_ALREADY_RESTORED",
                error.Code
            );

            Assert.Equal(
                ErrorType.Conflict,
                error.ErrorType
            );

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(30 * TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay + 1)]
        public void Undelete_PermanentlyDeleted_ReturnsExpiredFailureWithoutChangingState(long ticksFromCreated)
        {
            var entity = RestorePermanentlyDeleted(D);

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var result = entity.Undelete(T0.AddTicks(ticksFromCreated));

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                "USER_DELETION.RESTORATION_PERIOD_EXPIRED",
                error.Code
            );

            Assert.Equal(
                ErrorType.Forbidden,
                error.ErrorType
            );

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Fact]
        public void Undelete_RepeatedCall_PreservesFirstRestorationTimestamp()
        {
            var entity = RestoreDeleted();

            var first = entity.Undelete(T1);
            var second = entity.Undelete(T1.AddHours(1));

            Assert.True(first.IsSuccess);
            Assert.True(second.IsFailure);

            Assert.Equal(
                "USER_DELETION.USER_ALREADY_RESTORED",
                Assert.Single(second.Errors).Code
            );

            Assert.Equal(
                T1,
                entity.RestoredAt
            );

            Assert.Equal(
                T1,
                entity.UpdatedAt
            );
        }

        [Fact]
        public void Undelete_DeadlineWithDifferentOffset_ReturnsExpiredFailure()
        {
            var entity = RestoreDeleted();
            var now = D.ToOffset(TimeSpan.FromHours(2));

            var result = entity.Undelete(now);

            Assert.True(result.IsFailure);

            Assert.Equal(
                "USER_DELETION.RESTORATION_PERIOD_EXPIRED",
                Assert.Single(result.Errors).Code
            );
        }

        [Fact]
        public void Undelete_ValidTimeWithOffset_PreservesProvidedOffset()
        {
            var entity = RestoreDeleted();
            var now = T1.ToOffset(TimeSpan.FromHours(2));

            var result = entity.Undelete(now);

            Assert.True(result.IsSuccess);

            Assert.Equal(
                now,
                entity.RestoredAt
            );

            Assert.Equal(
                now.Offset,
                entity.RestoredAt!.Value.Offset
            );

            Assert.Equal(
                now,
                entity.UpdatedAt
            );

            Assert.Equal(
                now.Offset,
                entity.UpdatedAt!.Value.Offset
            );
        }

        [Theory]
        [InlineData(DeletionStatus.Deleted)]
        [InlineData(DeletionStatus.Restored)]
        [InlineData(DeletionStatus.PermanentlyDeleted)]
        public void TryMarkAsPermanentlyDeleted_NowBeforeCreatedAt_ThrowsWithoutChangingState(DeletionStatus status)
        {
            var entity = status switch
            {
                DeletionStatus.Deleted => RestoreDeleted(),
                DeletionStatus.Restored => RestoreRestored(T1),
                _ => RestorePermanentlyDeleted(D)
            };

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                entity.TryMarkAsPermanentlyDeleted(T0.AddTicks(-1))
            );

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
                "UserDeletion",
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
                T0,
                exception.Context["Custom_MinimumValue"]
            );

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TryMarkAsPermanentlyDeleted_DeletedBeforeDeadline_ReturnsFalseWithoutChangingState(long ticksFromDeadline)
        {
            var entity = RestoreDeleted();
            var now = ticksFromDeadline == 0 ? T0 : D.AddTicks(-1);

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var changed = entity.TryMarkAsPermanentlyDeleted(now);

            Assert.False(changed);

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void TryMarkAsPermanentlyDeleted_DeletedAtOrAfterDeadline_ChangesStateAndReturnsTrue(long ticksFromDeadline)
        {
            var entity = RestoreDeleted();
            var now = D.AddTicks(ticksFromDeadline);

            var changed = entity.TryMarkAsPermanentlyDeleted(now);

            Assert.True(changed);

            Assert.Equal(
                DeletionStatus.PermanentlyDeleted,
                entity.Status
            );

            Assert.Equal(
                now,
                entity.UpdatedAt
            );

            Assert.Null(entity.RestoredAt);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(30 * TimeSpan.TicksPerDay)]
        [InlineData(30 * TimeSpan.TicksPerDay + 1)]
        public void TryMarkAsPermanentlyDeleted_Restored_ReturnsFalseWithoutChangingState(long ticksFromCreated)
        {
            var entity = RestoreRestored(T1);

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var changed = entity.TryMarkAsPermanentlyDeleted(T0.AddTicks(ticksFromCreated));

            Assert.False(changed);

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void TryMarkAsPermanentlyDeleted_AlreadyPermanentlyDeleted_ReturnsFalseWithoutChangingState(long ticksAfterUpdatedAt)
        {
            var entity = RestorePermanentlyDeleted(D);

            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedLogin = entity.Login;
            var expectedReason = entity.Reason;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRestoreUntil = entity.RestoreUntil;
            var expectedRestoredAt = entity.RestoredAt;
            var expectedStatus = entity.Status;
            var expectedUpdatedAt = entity.UpdatedAt;

            var changed = entity.TryMarkAsPermanentlyDeleted(
                D.AddTicks(ticksAfterUpdatedAt)
            );

            Assert.False(changed);

            AssertUnchanged(
                entity,
                expectedId,
                expectedUserId,
                expectedLogin,
                expectedReason,
                expectedCreatedAt,
                expectedRestoreUntil,
                expectedRestoredAt,
                expectedStatus,
                expectedUpdatedAt
            );
        }

        [Fact]
        public void TryMarkAsPermanentlyDeleted_RepeatedCall_PreservesFirstUpdateTimestamp()
        {
            var entity = RestoreDeleted();

            Assert.True(entity.TryMarkAsPermanentlyDeleted(D));
            Assert.False(entity.TryMarkAsPermanentlyDeleted(D.AddHours(1)));

            Assert.Equal(
                D,
                entity.UpdatedAt
            );
        }

        [Fact]
        public void TryMarkAsPermanentlyDeleted_DeadlineWithDifferentOffset_MarksAsPermanentlyDeleted()
        {
            var entity = RestoreDeleted();
            var now = D.ToOffset(TimeSpan.FromHours(2));

            var changed = entity.TryMarkAsPermanentlyDeleted(now);

            Assert.True(changed);

            Assert.Equal(
                DeletionStatus.PermanentlyDeleted,
                entity.Status
            );

            Assert.Equal(
                now,
                entity.UpdatedAt
            );

            Assert.Equal(
                now.Offset,
                entity.UpdatedAt!.Value.Offset
            );
        }

        [Fact]
        public void Restore_StateProducedByDomainOperations_ReconstructsSameState()
        {
            var created = CreateDeleted();

            var restoredFromCreated = UserDeletion.Restore(
                created.Id,
                created.UserId,
                created.Login,
                created.Reason,
                created.CreatedAt,
                created.RestoreUntil,
                created.RestoredAt,
                created.Status,
                created.UpdatedAt
            );

            Assert.NotSame(
                created,
                restoredFromCreated
            );

            AssertUnchanged(
                restoredFromCreated,
                created.Id,
                created.UserId,
                created.Login,
                created.Reason,
                created.CreatedAt,
                created.RestoreUntil,
                created.RestoredAt,
                created.Status,
                created.UpdatedAt
            );

            var afterUndelete = CreateDeleted();

            Assert.True(afterUndelete.Undelete(T1).IsSuccess);

            var restoredAfterUndelete = UserDeletion.Restore(
                afterUndelete.Id,
                afterUndelete.UserId,
                afterUndelete.Login,
                afterUndelete.Reason,
                afterUndelete.CreatedAt,
                afterUndelete.RestoreUntil,
                afterUndelete.RestoredAt,
                afterUndelete.Status,
                afterUndelete.UpdatedAt
            );

            Assert.NotSame(
                afterUndelete,
                restoredAfterUndelete
            );

            AssertUnchanged(
                restoredAfterUndelete,
                afterUndelete.Id,
                afterUndelete.UserId,
                afterUndelete.Login,
                afterUndelete.Reason,
                afterUndelete.CreatedAt,
                afterUndelete.RestoreUntil,
                afterUndelete.RestoredAt,
                afterUndelete.Status,
                afterUndelete.UpdatedAt
            );

            var afterPermanent = CreateDeleted();

            Assert.True(afterPermanent.TryMarkAsPermanentlyDeleted(D));

            var restoredAfterPermanent = UserDeletion.Restore(
                afterPermanent.Id,
                afterPermanent.UserId,
                afterPermanent.Login,
                afterPermanent.Reason,
                afterPermanent.CreatedAt,
                afterPermanent.RestoreUntil,
                afterPermanent.RestoredAt,
                afterPermanent.Status,
                afterPermanent.UpdatedAt
            );

            Assert.NotSame(
                afterPermanent,
                restoredAfterPermanent
            );

            AssertUnchanged(
                restoredAfterPermanent,
                afterPermanent.Id,
                afterPermanent.UserId,
                afterPermanent.Login,
                afterPermanent.Reason,
                afterPermanent.CreatedAt,
                afterPermanent.RestoreUntil,
                afterPermanent.RestoredAt,
                afterPermanent.Status,
                afterPermanent.UpdatedAt
            );
        }
    }
}
