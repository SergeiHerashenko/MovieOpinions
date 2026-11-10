using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRestriction
{
    public class UserRestrictionTests
    {
        #region Test data
        // Фіксовані моменти задають початок, проміжний час і кінець дії правила.
        // Fixed instants define the start, an intermediate time, and the end of the rule duration.
        private static readonly DateTimeOffset T0 = new(
            2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset T1 = T0.AddMinutes(30);
        private static readonly DateTimeOffset AfterRule = T0.AddMinutes(60);
        #endregion

        #region Helpers: factories
        private static UserId CreateUserId()
            => UserId.Restore(Guid.Parse("11111111-1111-4111-8111-111111111111"));

        private static UserRestrictionId CreateRestrictionId()
            => UserRestrictionId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222"));

        private static RestrictionRule CreateRule(int durationMinutes = 60)
            => RestrictionRule.Restore(
                "LoginAttemptLimit",
                durationMinutes
            );

        private static UserRestriction CreateActive(
            RestrictionType type = RestrictionType.Ban,
            DateTimeOffset? now = null,
            string imposedBy = "System",
            string? reason = "Spam",
            UserId? userId = null,
            RestrictionRule? rule = null)
            => UserRestriction.Create(
                userId ?? CreateUserId(),
                type,
                rule ?? CreateRule(),
                imposedBy,
                reason,
                now ?? T0
            );

        private static UserRestriction RestoreRestriction(
            RestrictionStatus status,
            RestrictionType type = RestrictionType.Ban,
            DateTimeOffset? revokedAt = null,
            DateTimeOffset? createdAt = null,
            string imposedBy = "System",
            string? reason = "Spam",
            UserRestrictionId? id = null,
            UserId? userId = null,
            RestrictionRule? rule = null)
            => UserRestriction.Restore(
                id ?? CreateRestrictionId(),
                userId ?? CreateUserId(),
                rule ?? CreateRule(),
                type,
                imposedBy,
                reason,
                status,
                createdAt ?? T0,
                revokedAt
            );
        #endregion

        #region Helpers: state assertions
        // Знімаємо стан до виклику, щоб незалежно перевірити відсутність змін.
        // Capture state before the call to verify independently that nothing changed.
        private static void CaptureAndAssertUnchanged(
            UserRestriction entity,
            Action act)
        {
            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedRule = entity.RestrictionRule;
            var expectedType = entity.RestrictionType;
            var expectedReason = entity.Reason;
            var expectedImposedBy = entity.ImposedBy;
            var expectedStatus = entity.Status;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedRevokedAt = entity.RevokedAt;

            act();

            Assert.Same(
                expectedId,
                entity.Id
            );

            Assert.Same(
                expectedUserId,
                entity.UserId
            );

            Assert.Same(
                expectedRule,
                entity.RestrictionRule
            );

            Assert.Equal(
                expectedType,
                entity.RestrictionType
            );

            Assert.Equal(
                expectedReason,
                entity.Reason
            );

            Assert.Equal(
                expectedImposedBy,
                entity.ImposedBy
            );

            Assert.Equal(
                expectedStatus,
                entity.Status
            );

            Assert.Equal(
                expectedCreatedAt,
                entity.CreatedAt
            );

            Assert.Equal(
                expectedRevokedAt,
                entity.RevokedAt
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        private static void AssertImmutableProperties(
            UserRestriction entity,
            UserRestrictionId expectedId,
            UserId expectedUserId,
            RestrictionRule expectedRule,
            RestrictionType expectedType,
            string? expectedReason,
            string expectedImposedBy,
            DateTimeOffset expectedCreatedAt)
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
                expectedRule,
                entity.RestrictionRule
            );

            Assert.Equal(
                expectedType,
                entity.RestrictionType
            );

            Assert.Equal(
                expectedReason,
                entity.Reason
            );

            Assert.Equal(
                expectedImposedBy,
                entity.ImposedBy
            );

            Assert.Equal(
                expectedCreatedAt,
                entity.CreatedAt
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
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
                "UserRestriction",
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

            Assert.Equal(
                $"An error occurred during the '{operation}' operation. " +
                $"The field '{field}' in type 'UserRestriction' is empty!",
                exception.Message
            );
        }

        private static void AssertUnsupportedEnum(
            DomainDataInconsistencyException exception,
            string field,
            object expectedValue,
            string operation)
        {
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
                "UserRestriction",
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

            Assert.Equal(
                expectedValue,
                exception.Context["Value"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. ".Replace(
                    "Restore",
                    operation
                ) +
                $"Unsupported discriminator value '{expectedValue}' for type 'UserRestriction'!",
                exception.Message
            );
        }

        private static void AssertOutOfRangeField(
            DomainDataInconsistencyException exception,
            string field,
            string minimumField,
            DateTimeOffset minimumValue,
            string operation)
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
                "UserRestriction",
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

            Assert.Equal(
                "DateTimeOffset",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                minimumField,
                exception.Context["Custom_MinimumField"]
            );

            Assert.Equal(
                minimumValue,
                exception.Context["Custom_MinimumValue"]
            );

            Assert.Equal(
                $"An error occurred during the '{operation}' operation. " +
                $"The field '{field}' in type 'UserRestriction' has a value outside the allowed range!",
                exception.Message
            );
        }

        private static void AssertInvariant(
            DomainInvariantViolationException exception,
            string expectedRule,
            string operation)
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
                "UserRestriction",
                exception.Context["Type"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );

            Assert.Equal(
                expectedRule,
                exception.Context["Rule"]
            );

            Assert.Equal(
                $"Critical invariant violation in type 'UserRestriction'. Broken rule: {expectedRule}.",
                exception.Message
            );
        }

        private static void AssertTransitionInvariant(
            DomainInvariantViolationException exception,
            UserRestriction entity,
            RestrictionStatus expectedStatus,
            RestrictionStatus targetStatus)
        {
            AssertInvariant(
                exception,
                $"Only an active restriction can transition to the '{targetStatus}' state.",
                "Update"
            );

            Assert.Equal(
                entity.Id.Value,
                exception.Context["State_RestrictionId"]
            );

            Assert.Equal(
                entity.RestrictionType,
                exception.Context["State_RestrictionType"]
            );

            Assert.Equal(
                expectedStatus,
                exception.Context["State_RestrictionStatus"]
            );

            Assert.Equal(
                targetStatus,
                exception.Context["State_TargetStatus"]
            );
        }
        #endregion

        #region Create: valid state
        [Theory]
        [InlineData(RestrictionType.Ban)]
        [InlineData(RestrictionType.CommentMute)]
        [InlineData(RestrictionType.ChatMute)]
        [InlineData(RestrictionType.AllMute)]
        public void Create_ValidArguments_InitializesActiveRestriction(RestrictionType type)
        {
            var userId = CreateUserId();
            var rule = CreateRule();

            var now = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var entity = UserRestriction.Create(
                userId,
                type,
                rule,
                "System",
                "Spam",
                now
            );

            Assert.Same(
                userId,
                entity.UserId
            );

            Assert.Same(
                rule,
                entity.RestrictionRule
            );

            Assert.Equal(
                type,
                entity.RestrictionType
            );

            Assert.Equal(
                "System",
                entity.ImposedBy
            );

            Assert.Equal(
                "Spam",
                entity.Reason
            );

            Assert.NotEqual(
                Guid.Empty,
                entity.Id.Value
            );

            Assert.Equal(
                7,
                entity.Id.Value.Version
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
                RestrictionStatus.Active,
                entity.Status
            );

            Assert.Null(entity.RevokedAt);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData("System")]
        [InlineData(" Admin ")]
        [InlineData("\tAdmin\r\n")]
        [InlineData("Адміністратор")]
        [InlineData("A")]
        public void Create_ImposedBy_PreservesExactValue(string imposedBy)
        {
            var entity = CreateActive(imposedBy: imposedBy);

            Assert.Equal(
                imposedBy,
                entity.ImposedBy
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("Reason")]
        [InlineData(" Причина 🙂 ")]
        public void Create_Reason_PreservesExactValue(string? reason)
        {
            var entity = CreateActive(reason: reason);

            Assert.Equal(
                reason,
                entity.Reason
            );
        }
        #endregion

        #region Create: validation
        [Theory]
        [InlineData("userId")]
        [InlineData("restrictionRule")]
        public void Create_NullRequiredArgument_ThrowsDataInconsistency(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Create(
                    nullField == "userId" ? null! : CreateUserId(),
                    RestrictionType.Ban,
                    nullField == "restrictionRule" ? null! : CreateRule(),
                    "System",
                    "Spam",
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
        public void Create_BothRequiredArgumentsNull_ReportsUserId()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Create(
                    null!,
                    RestrictionType.Ban,
                    null!,
                    "System",
                    "Spam",
                    T0
                )
            );

            AssertEmptyField(
                exception,
                "userId",
                "Create"
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyImposedBy_ThrowsDataInconsistency(string? imposedBy)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                CreateActive(imposedBy: imposedBy!));

            AssertEmptyField(
                exception,
                "imposedBy",
                "Create"
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Create_UndefinedRestrictionType_ThrowsDataInconsistency(int typeValue)
        {
            var type = (RestrictionType)typeValue;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Create(
                    CreateUserId(),
                    type,
                    CreateRule(),
                    "System",
                    "Spam",
                    T0
                )
            );

            AssertUnsupportedEnum(
                exception,
                "restrictionType",
                type,
                "Create"
            );
        }

        [Fact]
        public void Create_ValidationOrder_ReportsFirstViolation()
        {
            var emptyVoFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Create(
                    null!,
                    (RestrictionType)999,
                    null!,
                    " ",
                    "Spam",
                    T0
                )
            );

            AssertEmptyField(
                emptyVoFirst,
                "userId",
                "Create"
            );

            var enumBeforeImposedBy = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Create(
                    CreateUserId(),
                    (RestrictionType)999,
                    CreateRule(),
                    " ",
                    "Spam",
                    T0
                )
            );

            AssertUnsupportedEnum(
                enumBeforeImposedBy,
                "restrictionType",
                (RestrictionType)999,
                "Create"
            );
        }
        #endregion

        #region Create: date limits and identity
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ExtremeCreationTimes_Succeeds(bool useMinValue)
        {
            var now = useMinValue
                ? DateTimeOffset.MinValue
                : DateTimeOffset.MaxValue;

            var entity = CreateActive(now: now);

            Assert.Equal(
                now,
                entity.CreatedAt
            );
        }

        [Fact]
        public void Create_SeparateCalls_GenerateDifferentIds()
        {
            var first = CreateActive();
            var second = CreateActive();

            Assert.NotEqual(
                first.Id.Value,
                second.Id.Value
            );
        }
        #endregion

        #region Restore: stored values
        [Theory]
        [InlineData(RestrictionStatus.Active, RestrictionType.Ban)]
        [InlineData(RestrictionStatus.Completed, RestrictionType.CommentMute)]
        [InlineData(RestrictionStatus.Revoked, RestrictionType.ChatMute)]
        [InlineData(RestrictionStatus.Active, RestrictionType.AllMute)]
        public void Restore_ValidState_PreservesAllArguments(
            RestrictionStatus status,
            RestrictionType type)
        {
            var id = CreateRestrictionId();
            var userId = CreateUserId();
            var rule = CreateRule();

            DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked
                ? T1.ToOffset(TimeSpan.FromHours(3))
                : null;

            var created = new DateTimeOffset(
                2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var entity = UserRestriction.Restore(
                id,
                userId,
                rule,
                type,
                "System",
                "Spam",
                status,
                created,
                revokedAt
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
                rule,
                entity.RestrictionRule
            );

            Assert.Equal(
                type,
                entity.RestrictionType
            );

            Assert.Equal(
                "System",
                entity.ImposedBy
            );

            Assert.Equal(
                "Spam",
                entity.Reason
            );

            Assert.Equal(
                status,
                entity.Status
            );

            Assert.Equal(
                created,
                entity.CreatedAt
            );

            Assert.Equal(
                created.Offset,
                entity.CreatedAt.Offset
            );

            Assert.Equal(
                revokedAt,
                entity.RevokedAt
            );

            Assert.Equal(
                revokedAt?.Offset,
                entity.RevokedAt?.Offset
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData("System")]
        [InlineData(" Admin ")]
        [InlineData("\tAdmin\r\n")]
        [InlineData("Адміністратор")]
        [InlineData("A")]
        public void Restore_ImposedBy_PreservesExactValue(string imposedBy)
        {
            var entity = RestoreRestriction(
                RestrictionStatus.Active,
                imposedBy: imposedBy
            );

            Assert.Equal(
                imposedBy,
                entity.ImposedBy
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("Reason")]
        [InlineData(" Причина 🙂 ")]
        public void Restore_Reason_PreservesExactValue(string? reason)
        {
            var entity = RestoreRestriction(
                RestrictionStatus.Active,
                reason: reason
            );

            Assert.Equal(
                reason,
                entity.Reason
            );
        }
        #endregion

        #region Restore: validation
        [Theory]
        [InlineData("userRestrictionId")]
        [InlineData("userId")]
        [InlineData("restrictionRule")]
        public void Restore_NullRequiredArgument_ThrowsDataInconsistency(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    nullField == "userRestrictionId" ? null! : CreateRestrictionId(),
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "restrictionRule" ? null! : CreateRule(),
                    RestrictionType.Ban,
                    "System",
                    "Spam",
                    RestrictionStatus.Active,
                    T0,
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
        [InlineData(true, true, false, "userRestrictionId")]
        [InlineData(true, false, true, "userRestrictionId")]
        [InlineData(false, true, true, "userId")]
        [InlineData(true, true, true, "userRestrictionId")]
        public void Restore_MultipleNullArguments_ReportsFirstNullArgument(
            bool nullId,
            bool nullUserId,
            bool nullRule,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    nullId ? null! : CreateRestrictionId(),
                    nullUserId ? null! : CreateUserId(),
                    nullRule ? null! : CreateRule(),
                    RestrictionType.Ban,
                    "System",
                    "Spam",
                    RestrictionStatus.Active,
                    T0,
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
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyImposedBy_ThrowsDataInconsistency(string? imposedBy)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestoreRestriction(
                    RestrictionStatus.Active,
                    imposedBy: imposedBy!
                )
            );

            AssertEmptyField(
                exception,
                "imposedBy",
                "Restore"
            );
        }

        [Theory]
        [InlineData("restrictionType", -1)]
        [InlineData("restrictionType", 999)]
        [InlineData("restrictionStatus", -1)]
        [InlineData("restrictionStatus", 999)]
        public void Restore_UndefinedEnum_ThrowsDataInconsistency(
            string field,
            int enumValue)
        {
            var type = field == "restrictionType"
                ? (RestrictionType)enumValue
                : RestrictionType.Ban;

            var status = field == "restrictionStatus"
                ? (RestrictionStatus)enumValue
                : RestrictionStatus.Active;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    CreateRestrictionId(),
                    CreateUserId(),
                    CreateRule(),
                    type,
                    "System",
                    "Spam",
                    status,
                    T0,
                    null
                )
            );

            AssertUnsupportedEnum(
                exception,
                field,
                field == "restrictionType" ? type : status,
                "Restore"
            );
        }

        [Fact]
        public void Restore_RevokedAtBeforeCreation_ThrowsDataInconsistency()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestoreRestriction(
                    RestrictionStatus.Revoked,
                    revokedAt: T0.AddTicks(-1)
                )
            );

            AssertOutOfRangeField(
                exception,
                "revokedAt",
                "createdAt",
                T0,
                "Restore"
            );
        }

        [Theory]
        [InlineData(RestrictionStatus.Active, true)]
        [InlineData(RestrictionStatus.Completed, true)]
        [InlineData(RestrictionStatus.Revoked, false)]
        public void Restore_InconsistentState_ThrowsInvariantViolation(
            RestrictionStatus status,
            bool hasRevokedAt)
        {
            DateTimeOffset? revokedAt = hasRevokedAt ? T1 : null;

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestoreRestriction(
                    status,
                    revokedAt: revokedAt
                )
            );

            AssertInvariant(
                exception,
                "Inconsistent restriction state: fields 'revokedAt' and 'restrictionStatus' are not consistent!",
                "Restore"
            );

            Assert.Equal(
                status,
                exception.Context["State_RestrictionStatus"]
            );

            Assert.Equal(
                (object?)revokedAt ?? "null",
                exception.Context["State_RevokedAt"]
            );
        }
        #endregion

        #region Restore: revocation time boundaries
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void Restore_RevokedAtAtOrAfterCreation_Succeeds(long ticksFromCreated)
        {
            var revokedAt = T0.AddTicks(ticksFromCreated);

            var entity = RestoreRestriction(
                RestrictionStatus.Revoked,
                revokedAt: revokedAt
            );

            Assert.Equal(
                RestrictionStatus.Revoked,
                entity.Status
            );

            Assert.Equal(
                revokedAt,
                entity.RevokedAt
            );
        }

        // Різні UTC-зсуви можуть представляти той самий момент відкликання.
        // Different UTC offsets can represent the same revocation instant.
        [Fact]
        public void Restore_RevokedAtAtCreationWithDifferentOffset_Succeeds()
        {
            var revokedAt = T0.ToOffset(TimeSpan.FromHours(2));

            var entity = RestoreRestriction(
                RestrictionStatus.Revoked,
                revokedAt: revokedAt
            );

            Assert.Equal(
                revokedAt,
                entity.RevokedAt
            );

            Assert.Equal(
                revokedAt.Offset,
                entity.RevokedAt!.Value.Offset
            );
        }
        #endregion

        #region Restore: validation order and date limits
        [Fact]
        public void Restore_ValidationOrder_ReportsFirstViolation()
        {
            var emptyFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    null!,
                    CreateUserId(),
                    CreateRule(),
                    RestrictionType.Ban,
                    " ",
                    "Spam",
                    RestrictionStatus.Active,
                    T0,
                    null
                )
            );

            AssertEmptyField(
                emptyFirst,
                "userRestrictionId",
                "Restore"
            );

            var imposedByFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    CreateRestrictionId(),
                    CreateUserId(),
                    CreateRule(),
                    (RestrictionType)999,
                    " ",
                    "Spam",
                    RestrictionStatus.Active,
                    T0,
                    null
                )
            );

            AssertEmptyField(
                imposedByFirst,
                "imposedBy",
                "Restore"
            );

            var typeFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    CreateRestrictionId(),
                    CreateUserId(),
                    CreateRule(),
                    (RestrictionType)999,
                    "System",
                    "Spam",
                    (RestrictionStatus)999,
                    T0,
                    null
                )
            );

            AssertUnsupportedEnum(
                typeFirst,
                "restrictionType",
                (RestrictionType)999,
                "Restore"
            );

            var statusBeforeChronology =
                Assert.Throws<DomainDataInconsistencyException>(() =>
                    UserRestriction.Restore(
                        CreateRestrictionId(),
                        CreateUserId(),
                        CreateRule(),
                        RestrictionType.Ban,
                        "System",
                        "Spam",
                        (RestrictionStatus)999,
                        T0,
                        T0.AddTicks(-1)
                    )
                );

            AssertUnsupportedEnum(
                statusBeforeChronology,
                "restrictionStatus",
                (RestrictionStatus)999,
                "Restore"
            );

            var chronologyFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestriction.Restore(
                    CreateRestrictionId(),
                    CreateUserId(),
                    CreateRule(),
                    RestrictionType.Ban,
                    "System",
                    "Spam",
                    RestrictionStatus.Active,
                    T0,
                    T0.AddTicks(-1)
                )
            );

            AssertOutOfRangeField(
                chronologyFirst,
                "revokedAt",
                "createdAt",
                T0,
                "Restore"
            );
        }

        [Theory]
        [InlineData(RestrictionStatus.Active)]
        [InlineData(RestrictionStatus.Completed)]
        [InlineData(RestrictionStatus.Revoked)]
        public void Restore_ExtremeDates_Succeeds(RestrictionStatus status)
        {
            foreach (var created in new[]
                     {
                         DateTimeOffset.MinValue,
                         DateTimeOffset.MaxValue
                     })
            {
                DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked
                    ? created
                    : null;

                var entity = RestoreRestriction(
                    status,
                    createdAt: created,
                    revokedAt: revokedAt
                );

                Assert.Equal(
                    status,
                    entity.Status
                );

                Assert.Equal(
                    created,
                    entity.CreatedAt
                );

                Assert.Equal(
                    revokedAt,
                    entity.RevokedAt
                );
            }
        }
        #endregion

        #region ValidateCanComplete
        [Theory]
        [InlineData(0)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        public void ValidateCanComplete_ActiveAtOrAfterCreation_DoesNotThrowAndPreservesState(int minutesFromCreated)
        {
            var entity = RestoreRestriction(RestrictionStatus.Active);
            var now = T0.AddMinutes(minutesFromCreated);

            CaptureAndAssertUnchanged(entity, () =>
                entity.ValidateCanComplete(now));
        }

        [Theory]
        [InlineData(RestrictionStatus.Completed)]
        [InlineData(RestrictionStatus.Revoked)]
        public void ValidateCanComplete_TerminalStatus_ThrowsInvariantAndPreservesState(RestrictionStatus status)
        {
            DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked ? T1 : null;

            var entity = RestoreRestriction(
                status,
                revokedAt: revokedAt
            );

            foreach (var now in new[] { T0, T1, AfterRule.AddTicks(1) })
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                        entity.ValidateCanComplete(now));

                    AssertInvariant(
                        exception,
                        "Only an active restriction can transition to the 'Completed' state.",
                        "Update"
                    );

                    Assert.Equal(
                        entity.Id.Value,
                        exception.Context["State_RestrictionId"]
                    );

                    Assert.Equal(
                        entity.RestrictionType,
                        exception.Context["State_RestrictionType"]
                    );

                    Assert.Equal(
                        status,
                        exception.Context["State_RestrictionStatus"]
                    );
                });
            }
        }

        [Theory]
        [InlineData(RestrictionStatus.Active)]
        [InlineData(RestrictionStatus.Completed)]
        [InlineData(RestrictionStatus.Revoked)]
        public void ValidateCanComplete_BeforeCreation_ThrowsDataInconsistency(RestrictionStatus status)
        {
            DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked ? T1 : null;

            var entity = RestoreRestriction(
                status,
                revokedAt: revokedAt
            );

            var now = T0.AddTicks(-1);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    entity.ValidateCanComplete(now));

                AssertOutOfRangeField(
                    exception,
                    "now",
                    "CreatedAt",
                    T0,
                    "Update"
                );
            });
        }
        #endregion

        #region ValidateRevocation
        [Theory]
        [InlineData(0)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        public void ValidateRevocation_ActiveAtOrAfterCreation_ReturnsSuccessAndPreservesState(int minutesFromCreated)
        {
            var entity = RestoreRestriction(RestrictionStatus.Active);
            var now = T0.AddMinutes(minutesFromCreated);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var result = entity.ValidateRevocation(now);

                Assert.True(result.IsSuccess);
                Assert.Empty(result.Errors);
            });
        }

        [Theory]
        [InlineData(RestrictionStatus.Completed, "RESTRICTION.ALREADY_COMPLETED",
            "Restriction status transition failed for type 'UserRestriction': " +
            "the restriction has already been completed.")]
        [InlineData(RestrictionStatus.Revoked, "RESTRICTION.ALREADY_REVOKED",
            "Restriction status transition failed for type 'UserRestriction': " +
            "the restriction has already been revoked.")]
        public void ValidateRevocation_TerminalStatus_ReturnsSpecificFailureAndPreservesState(
            RestrictionStatus status,
            string expectedCode,
            string expectedMessage)
        {
            DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked ? T1 : null;

            var entity = RestoreRestriction(
                status,
                revokedAt: revokedAt
            );

            foreach (var now in new[] { T0, T1, AfterRule, AfterRule.AddTicks(1) })
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var result = entity.ValidateRevocation(now);

                    Assert.True(result.IsFailure);

                    var error = Assert.Single(result.Errors);

                    Assert.Equal(
                        ErrorType.Conflict,
                        error.ErrorType
                    );

                    Assert.Equal(
                        expectedCode,
                        error.Code
                    );

                    Assert.Equal(
                        expectedMessage,
                        error.Message
                    );
                });
            }
        }

        [Theory]
        [InlineData(RestrictionStatus.Active)]
        [InlineData(RestrictionStatus.Completed)]
        [InlineData(RestrictionStatus.Revoked)]
        public void ValidateRevocation_BeforeCreation_ThrowsDataInconsistency(RestrictionStatus status)
        {
            DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked ? T1 : null;
            var entity = RestoreRestriction(
                status,
                revokedAt: revokedAt
            );

            var now = T0.AddTicks(-1);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    entity.ValidateRevocation(now));

                AssertOutOfRangeField(
                    exception,
                    "now",
                    "CreatedAt",
                    T0,
                    "Update"
                );
            });
        }
        #endregion

        #region Complete and Revoke
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompleteOrRevoke_ActiveAtOrAfterCreation_AppliesExpectedState(bool complete)
        {
            foreach (var now in new[]
             {
                 T0,
                 T0.AddTicks(1),
                 T1,
                 AfterRule,
                 AfterRule.AddMinutes(1),
                 T0.ToOffset(TimeSpan.FromHours(-2))
             })
            {
                var entity = RestoreRestriction(RestrictionStatus.Active);
                var expectedId = entity.Id;
                var expectedUserId = entity.UserId;
                var expectedRule = entity.RestrictionRule;
                var expectedType = entity.RestrictionType;
                var expectedReason = entity.Reason;
                var expectedImposedBy = entity.ImposedBy;
                var expectedCreatedAt = entity.CreatedAt;

                if (complete)
                {
                    entity.MarkAsCompleted(now);
                }
                else
                {
                    entity.RevokeRestriction(now);
                }

                Assert.Equal(
                    complete
                        ? RestrictionStatus.Completed
                        : RestrictionStatus.Revoked,
                    entity.Status
                );

                if (complete)
                {
                    Assert.Null(entity.RevokedAt);
                }
                else
                {
                    Assert.Equal(
                        now,
                        entity.RevokedAt
                    );

                    Assert.Equal(
                        now.Offset,
                        entity.RevokedAt!.Value.Offset
                    );
                }

                AssertImmutableProperties(
                    entity,
                    expectedId,
                    expectedUserId,
                    expectedRule,
                    expectedType,
                    expectedReason,
                    expectedImposedBy,
                    expectedCreatedAt
                );
            }
        }

        [Theory]
        [InlineData(true, RestrictionStatus.Completed)]
        [InlineData(true, RestrictionStatus.Revoked)]
        [InlineData(false, RestrictionStatus.Completed)]
        [InlineData(false, RestrictionStatus.Revoked)]
        public void CompleteOrRevoke_TerminalStatus_ThrowsInvariantAndPreservesState(
            bool complete,
            RestrictionStatus status)
        {
            DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked ? T1 : null;

            var entity = RestoreRestriction(
                status,
                revokedAt: revokedAt
            );

            var target = complete
                ? RestrictionStatus.Completed
                : RestrictionStatus.Revoked;

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (complete)
                    {
                        entity.MarkAsCompleted(T1);
                    }
                    else
                    {
                        entity.RevokeRestriction(T1);
                    }
                });

                AssertTransitionInvariant(
                    exception,
                    entity,
                    status,
                    target
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompleteOrRevoke_BeforeCreation_ThrowsDataInconsistency(bool complete)
        {
            foreach (var status in new[]
                     {
                         RestrictionStatus.Active,
                         RestrictionStatus.Completed,
                         RestrictionStatus.Revoked
                     })
            {
                DateTimeOffset? revokedAt = status == RestrictionStatus.Revoked
                    ? T1
                    : null;

                var entity = RestoreRestriction(
                    status,
                    revokedAt: revokedAt
                );

                var now = T0.AddTicks(-1);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    {
                        if (complete)
                        {
                            entity.MarkAsCompleted(now);
                        }
                        else
                        {
                            entity.RevokeRestriction(now);
                        }
                    });

                    AssertOutOfRangeField(
                        exception,
                        "now",
                        "CreatedAt",
                        T0,
                        "Update"
                    );
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompleteOrRevoke_RepeatedCall_ThrowsInvariantAndKeepsFirstState(bool complete)
        {
            var entity = RestoreRestriction(RestrictionStatus.Active);

            var target = complete
                ? RestrictionStatus.Completed
                : RestrictionStatus.Revoked;

            if (complete)
            {
                entity.MarkAsCompleted(T1);
            }
            else
            {
                entity.RevokeRestriction(T1);
            }

            var firstRevokedAt = entity.RevokedAt;

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (complete)
                    {
                        entity.MarkAsCompleted(T1.AddHours(1));
                    }
                    else
                    {
                        entity.RevokeRestriction(T1.AddHours(1));
                    }
                });

                AssertTransitionInvariant(
                    exception,
                    entity,
                    target,
                    target
                );
            });

            Assert.Equal(
                target,
                entity.Status
            );

            Assert.Equal(
                firstRevokedAt,
                entity.RevokedAt
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompleteOrRevoke_OtherTransitionAfterSuccess_ThrowsInvariantAndPreservesState(bool completeFirst)
        {
            var entity = RestoreRestriction(RestrictionStatus.Active);

            if (completeFirst)
            {
                entity.MarkAsCompleted(T1);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                        entity.RevokeRestriction(T1.AddHours(1)));

                    AssertTransitionInvariant(
                        exception,
                        entity,
                        RestrictionStatus.Completed,
                        RestrictionStatus.Revoked
                    );
                });
            }
            else
            {
                entity.RevokeRestriction(T1);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                        entity.MarkAsCompleted(T1.AddHours(1)));

                    AssertTransitionInvariant(
                        exception,
                        entity,
                        RestrictionStatus.Revoked,
                        RestrictionStatus.Completed
                    );
                });
            }
        }
        #endregion
    }
}
