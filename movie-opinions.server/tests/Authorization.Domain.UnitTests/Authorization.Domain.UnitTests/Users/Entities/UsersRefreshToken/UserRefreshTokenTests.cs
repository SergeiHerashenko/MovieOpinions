using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRefreshToken
{
    public class UserRefreshTokenTests
    {
        private static readonly DateTimeOffset T0 = new(
            2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset D = T0.AddDays(7);
        private static readonly DateTimeOffset Mid = T0.AddDays(3);

        private static UserId CreateUserId()
            => UserId.Restore(Guid.Parse("11111111-1111-4111-8111-111111111111"));

        private static UserRefreshTokenId CreateTokenId()
            => UserRefreshTokenId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222")
            );

        private static RefreshToken CreateRefreshToken(byte fill = 0)
        {
            var bytes = new byte[64];
            Array.Fill(bytes, fill);
            return RefreshToken.Restore(Convert.ToBase64String(bytes));
        }

        private static DeviceInfo CreateDeviceInfo()
            => DeviceInfo.Restore(
                DeviceType.Desktop,
                "Windows 11",
                "Chrome",
                "Model X");

        private static IpAddress CreateIp()
            => IpAddress.Restore("192.168.1.1");

        private static UserRefreshToken CreateActive(
            DateTimeOffset? now = null,
            string? city = null)
            => UserRefreshToken.Create(
                CreateUserId(),
                CreateDeviceInfo(),
                CreateIp(),
                now ?? T0,
                city
            );

        private static UserRefreshToken RestoreToken(
            TokenStatus status,
            DateTimeOffset? consumedAt = null,
            DateTimeOffset? revokedAt = null,
            DateTimeOffset? createdAt = null,
            DateTimeOffset? expiresAt = null,
            string? city = null,
            UserRefreshTokenId? id = null,
            UserId? userId = null,
            RefreshToken? refreshToken = null,
            DeviceInfo? deviceInfo = null,
            IpAddress? ipAddress = null)
        {
            var created = createdAt ?? T0;

            return UserRefreshToken.Restore(
                id ?? CreateTokenId(),
                userId ?? CreateUserId(),
                refreshToken ?? CreateRefreshToken(),
                deviceInfo ?? CreateDeviceInfo(),
                ipAddress ?? CreateIp(),
                city,
                status,
                expiresAt ?? created.AddDays(7),
                consumedAt,
                created,
                revokedAt
            );
        }

        private static void CaptureAndAssertUnchanged(
            UserRefreshToken entity,
            Action act)
        {
            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedToken = entity.RefreshToken;
            var expectedDevice = entity.DeviceInfo;
            var expectedIp = entity.IpAddress;
            var expectedCity = entity.City;
            var expectedStatus = entity.TokenStatus;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedExpiresAt = entity.ExpiresAt;
            var expectedConsumedAt = entity.ConsumedAt;
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
                expectedToken,
                entity.RefreshToken
            );

            Assert.Same(
                expectedDevice,
                entity.DeviceInfo
            );

            Assert.Same(
                expectedIp,
                entity.IpAddress
            );

            Assert.Equal(
                expectedCity,
                entity.City
            );

            Assert.Equal(
                expectedStatus,
                entity.TokenStatus
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
                expectedConsumedAt,
                entity.ConsumedAt
            );

            Assert.Equal(
                expectedRevokedAt,
                entity.RevokedAt
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

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
                "UserRefreshToken",
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
                $"The field '{field}' in type 'UserRefreshToken' is empty!",
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
                "UserRefreshToken",
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
                $"The field '{field}' in type 'UserRefreshToken' has a value outside the allowed range!",
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
                "UserRefreshToken",
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
                $"Critical invariant violation in type 'UserRefreshToken'. Broken rule: {expectedRule}.",
                exception.Message
            );
        }

        private static void AssertInconsistentStatusInvariant(
            DomainInvariantViolationException exception,
            TokenStatus expectedStatus,
            DateTimeOffset expectedExpiresAt,
            DateTimeOffset? expectedConsumedAt,
            DateTimeOffset? expectedRevokedAt)
        {
            AssertInvariant(
                exception,
                "Inconsistent state: fields 'tokenStatus' and 'consumedAt' and 'revokedAt' are not consistent!",
                "Restore"
            );

            Assert.Equal(
                expectedStatus,
                exception.Context["State_TokenStatus"]
            );

            Assert.Equal(
                expectedExpiresAt,
                exception.Context["State_ExpiresAt"]
            );

            Assert.Equal(
                (object?)expectedConsumedAt ?? "null",
                exception.Context["State_ConsumedAt"]
            );

            Assert.Equal(
                (object?)expectedRevokedAt ?? "null",
                exception.Context["State_RevokedAt"]
            );
        }

        private static void AssertInvalidStatusTransition(
            Result result,
            TokenStatus currentStatus,
            TokenStatus targetStatus)
        {
            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Conflict,
                error.ErrorType
            );

            Assert.Equal(
                "TOKEN_STATUS.INVALID_STATUS_TRANSITION",
                error.Code
            );

            Assert.Equal(
                "Refresh token status transition failed for type 'UserRefreshToken': " +
                $"transition from '{currentStatus}' to '{targetStatus}' is not allowed",
                error.Message
            );
        }

        private static void AssertTerminatePreconditionFailed(
            DomainInvalidOperationException exception,
            UserRefreshToken entity,
            TokenStatus expectedStatus,
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
                "UserRefreshToken",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Terminate",
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                "The refresh token must have Active status before termination.",
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                entity.Id.Value,
                exception.Context["Custom_RefreshTokenId"]
            );

            Assert.Equal(
                expectedStatus,
                exception.Context["Custom_TokenStatus"]
            );

            Assert.Equal(
                now,
                exception.Context["Custom_OperationTime"]
            );

            Assert.Equal(
                "Precondition failed in 'Terminate' for type 'UserRefreshToken'. " +
                "Required prerequisite " +
                "'The refresh token must have Active status before termination.' was not satisfied.",
                exception.Message
            );
        }

        private static void AssertImmutableProperties(
            UserRefreshToken entity,
            UserRefreshTokenId expectedId,
            UserId expectedUserId,
            RefreshToken expectedToken,
            DeviceInfo expectedDevice,
            IpAddress expectedIp,
            string? expectedCity,
            DateTimeOffset expectedCreatedAt,
            DateTimeOffset expectedExpiresAt)
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
                entity.RefreshToken
            );

            Assert.Same(
                expectedDevice,
                entity.DeviceInfo
            );

            Assert.Same(
                expectedIp,
                entity.IpAddress
            );

            Assert.Equal(
                expectedCity,
                entity.City
            );

            Assert.Equal(
                expectedCreatedAt,
                entity.CreatedAt
            );

            Assert.Equal(
                expectedExpiresAt,
                entity.ExpiresAt
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Fact]
        public void Create_ValidArguments_InitializesActiveToken()
        {
            var userId = CreateUserId();
            var deviceInfo = CreateDeviceInfo();
            var ip = CreateIp();
            var now = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var entity = UserRefreshToken.Create(userId, deviceInfo, ip, now);

            Assert.Same(
                userId,
                entity.UserId
            );

            Assert.Same(
                deviceInfo,
                entity.DeviceInfo
            );

            Assert.Same(
                ip,
                entity.IpAddress
            );

            Assert.NotEqual(
                Guid.Empty,
                entity.Id.Value
            );

            Assert.Equal(
                7,
                entity.Id.Value.Version
            );

            Assert.False(string.IsNullOrWhiteSpace(entity.RefreshToken.Value));

            Assert.Equal(
                now,
                entity.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                entity.CreatedAt.Offset
            );

            Assert.Equal(
                now.AddDays(7),
                entity.ExpiresAt
            );

            Assert.Equal(
                now.Offset,
                entity.ExpiresAt.Offset
            );

            Assert.Equal(
                TokenStatus.Active,
                entity.TokenStatus
            );

            Assert.Null(entity.ConsumedAt);
            Assert.Null(entity.RevokedAt);
            Assert.Null(entity.City);
            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("Київ")]
        [InlineData(" Київ ")]
        [InlineData("Київ 🙂")]
        public void Create_City_PreservesExactValue(string? city)
        {
            var entity = CreateActive(city: city);

            Assert.Equal(
                city,
                entity.City
            );
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
                first.RefreshToken.Value,
                second.RefreshToken.Value
            );
        }

        [Theory]
        [InlineData("userId")]
        [InlineData("deviceInfo")]
        [InlineData("ipAddress")]
        public void Create_NullRequiredArgument_ThrowsDataInconsistency(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Create(
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "deviceInfo" ? null! : CreateDeviceInfo(),
                    nullField == "ipAddress" ? null! : CreateIp(),
                    T0
                )
            );

            AssertEmptyField(
                exception,
                nullField,
                "Create"
            );
        }

        [Theory]
        [InlineData(true, true, false, "userId")]
        [InlineData(true, false, true, "userId")]
        [InlineData(false, true, true, "deviceInfo")]
        [InlineData(true, true, true, "userId")]
        public void Create_MultipleNullArguments_ReportsFirstNullArgument(
            bool nullUserId,
            bool nullDevice,
            bool nullIp,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Create(
                    nullUserId ? null! : CreateUserId(),
                    nullDevice ? null! : CreateDeviceInfo(),
                    nullIp ? null! : CreateIp(),
                    T0
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
        public void Create_ExtremeValidCreationTimes_Succeeds(bool useMinValue)
        {
            var now = useMinValue
                ? DateTimeOffset.MinValue
                : DateTimeOffset.MaxValue.AddDays(-7);

            var entity = CreateActive(now);

            Assert.Equal(
                now,
                entity.CreatedAt
            );

            Assert.Equal(
                now.AddDays(7),
                entity.ExpiresAt
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        public void Create_ExpirationOverflow_ThrowsArgumentOutOfRangeException(long ticksFromBoundary)
        {
            var now = ticksFromBoundary == 0
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.MaxValue.AddDays(-7).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateActive(now));
        }

        [Theory]
        [InlineData(TokenStatus.Active)]
        [InlineData(TokenStatus.Expired)]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        public void Restore_ValidState_PreservesAllArguments(TokenStatus status)
        {
            var id = CreateTokenId();
            var userId = CreateUserId();
            var token = CreateRefreshToken();
            var device = CreateDeviceInfo();
            var ip = CreateIp();

            DateTimeOffset? consumedAt = status == TokenStatus.Consumed
                ? Mid.ToOffset(TimeSpan.FromHours(3))
                : null;

            DateTimeOffset? revokedAt = status == TokenStatus.Revoked
                ? Mid.ToOffset(TimeSpan.FromHours(-3))
                : null;

            var created = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));
            var expires = created.AddDays(7);

            var entity = UserRefreshToken.Restore(
                id, userId, token, device, ip, "Київ",
                status, expires, consumedAt, created, revokedAt
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
                entity.RefreshToken
            );

            Assert.Same(
                device,
                entity.DeviceInfo
            );

            Assert.Same(
                ip,
                entity.IpAddress
            );

            Assert.Equal(
                "Київ",
                entity.City
            );

            Assert.Equal(
                status,
                entity.TokenStatus
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
                expires,
                entity.ExpiresAt
            );

            Assert.Equal(
                expires.Offset,
                entity.ExpiresAt.Offset
            );

            Assert.Equal(
                consumedAt,
                entity.ConsumedAt
            );

            Assert.Equal(
                revokedAt,
                entity.RevokedAt
            );

            Assert.Equal(
                consumedAt?.Offset,
                entity.ConsumedAt?.Offset
            );

            Assert.Equal(
                revokedAt?.Offset,
                entity.RevokedAt?.Offset
            );

            Assert.Empty(entity.DomainEvents);
            Assert.Empty(entity.AggregateChanges);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("Київ")]
        [InlineData(" Київ ")]
        [InlineData("Київ 🙂")]
        public void Restore_City_PreservesExactValue(string? city)
        {
            var entity = RestoreToken(
                TokenStatus.Active,
                city: city
            );

            Assert.Equal(
                city,
                entity.City
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerHour)]
        [InlineData(TimeSpan.TicksPerDay * 30)]
        public void Restore_AnyPositiveLifetime_Succeeds(long durationTicks)
        {
            var expiresAt = T0.AddTicks(durationTicks);

            var entity = RestoreToken(
                TokenStatus.Active,
                expiresAt: expiresAt
            );

            Assert.Equal(
                expiresAt,
                entity.ExpiresAt
            );
        }

        [Theory]
        [InlineData("userRefreshTokenId")]
        [InlineData("userId")]
        [InlineData("refreshToken")]
        [InlineData("deviceInfo")]
        [InlineData("ipAddress")]
        public void Restore_NullRequiredArgument_ThrowsDataInconsistency(string nullField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    nullField == "userRefreshTokenId" ? null! : CreateTokenId(),
                    nullField == "userId" ? null! : CreateUserId(),
                    nullField == "refreshToken" ? null! : CreateRefreshToken(),
                    nullField == "deviceInfo" ? null! : CreateDeviceInfo(),
                    nullField == "ipAddress" ? null! : CreateIp(),
                    null,
                    TokenStatus.Active,
                    D, null, T0, null
                )
            );

            AssertEmptyField(
                exception,
                nullField,
                "Restore"
            );
        }

        [Theory]
        [InlineData(true, true, true, true, true, "userRefreshTokenId")]
        [InlineData(false, true, true, true, true, "userId")]
        [InlineData(false, false, true, true, true, "refreshToken")]
        [InlineData(false, false, false, true, true, "deviceInfo")]
        public void Restore_MultipleNullArguments_ReportsFirstNullArgument(
            bool nullId,
            bool nullUserId,
            bool nullToken,
            bool nullDevice,
            bool nullIp,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    nullId ? null! : CreateTokenId(),
                    nullUserId ? null! : CreateUserId(),
                    nullToken ? null! : CreateRefreshToken(),
                    nullDevice ? null! : CreateDeviceInfo(),
                    nullIp ? null! : CreateIp(),
                    null,
                    TokenStatus.Active,
                    D, null, T0, null
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
        public void Restore_UndefinedStatus_ThrowsDataInconsistency(int statusValue)
        {
            var status = (TokenStatus)statusValue;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    CreateTokenId(), CreateUserId(), CreateRefreshToken(),
                    CreateDeviceInfo(), CreateIp(), null,
                    status, D, null, T0, null
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
                "UserRefreshToken",
                exception.Context["Type"]
            );

            Assert.Equal(
                "tokenStatus",
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

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                $"Unsupported discriminator value '{status}' for type 'UserRefreshToken'!",
                exception.Message
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Restore_ExpiresAtNotAfterCreatedAt_ThrowsInvariantViolation(long ticksOffset)
        {
            var expiresAt = ticksOffset == 0 ? T0 : T0.AddTicks(-1);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestoreToken(
                    TokenStatus.Active,
                    expiresAt: expiresAt
                )
            );

            AssertInvariant(
                exception,
                "Expiration time must be later than creation time.",
                "Restore"
            );

            Assert.Equal(
                T0,
                exception.Context["State_CreatedAt"]
            );

            Assert.Equal(
                expiresAt,
                exception.Context["State_ExpiresAt"]
            );
        }

        [Theory]
        [InlineData("consumedAt")]
        [InlineData("revokedAt")]
        public void Restore_LifecycleTimestampBeforeCreation_ThrowsDataInconsistency(string field)
        {
            var early = T0.AddTicks(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestoreToken(
                    field == "consumedAt"
                        ? TokenStatus.Consumed
                        : TokenStatus.Revoked,
                    consumedAt: field == "consumedAt" ? early : null,
                    revokedAt: field == "revokedAt" ? early : null
                )
            );

            AssertOutOfRangeField(
                exception,
                field,
                "createdAt",
                T0,
                "Restore"
            );
        }

        [Fact]
        public void Restore_BothLifecycleTimestampsBeforeCreation_ReportsConsumedAt()
        {
            var early = T0.AddTicks(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    CreateTokenId(), CreateUserId(), CreateRefreshToken(),
                    CreateDeviceInfo(), CreateIp(), null,
                    TokenStatus.Consumed,
                    D, early, T0, early
                )
            );

            AssertOutOfRangeField(
                exception,
                "consumedAt",
                "createdAt",
                T0,
                "Restore"
            );
        }

        public static TheoryData<TokenStatus, bool, bool> InconsistentPresenceCases()
        {
            var data = new TheoryData<TokenStatus, bool, bool>();

            foreach (var status in new[]
                     {
                        TokenStatus.Active,
                        TokenStatus.Expired,
                        TokenStatus.Consumed,
                        TokenStatus.Revoked
                     })
            {
                for (var c = 0; c <= 1; c++)
                {
                    for (var r = 0; r <= 1; r++)
                    {
                        var hasConsumed = c == 1;
                        var hasRevoked = r == 1;

                        var isValid = (status, hasConsumed, hasRevoked) switch
                        {
                            (TokenStatus.Active, false, false) => true,
                            (TokenStatus.Expired, false, false) => true,
                            (TokenStatus.Consumed, true, false) => true,
                            (TokenStatus.Revoked, false, true) => true,
                            _ => false
                        };

                        if (!isValid)
                        {
                            data.Add(status, hasConsumed, hasRevoked);
                        }
                    }
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(InconsistentPresenceCases))]
        public void Restore_InconsistentTimestampPresence_ThrowsInvariantViolation(
            TokenStatus status,
            bool hasConsumed,
            bool hasRevoked)
        {
            DateTimeOffset? consumedAt = hasConsumed ? Mid : null;
            DateTimeOffset? revokedAt = hasRevoked ? Mid : null;

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestoreToken(
                    status,
                    consumedAt,
                    revokedAt
                )
            );

            AssertInconsistentStatusInvariant(
                exception,
                status,
                D,
                consumedAt,
                revokedAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void Restore_ConsumedAtInsideLifetime_Succeeds(long ticksFromBoundary)
        {
            var consumedAt = ticksFromBoundary switch
            {
                0 => T0,
                1 => T0.AddTicks(1),
                _ => D.AddTicks(-1)
            };

            var entity = RestoreToken(
                TokenStatus.Consumed,
                consumedAt: consumedAt
            );

            Assert.Equal(
                TokenStatus.Consumed,
                entity.TokenStatus
            );

            Assert.Equal(
                consumedAt,
                entity.ConsumedAt
            );
        }

        [Fact]
        public void Restore_ConsumedAtAtCreationWithDifferentOffset_Succeeds()
        {
            var consumedAt = T0.ToOffset(TimeSpan.FromHours(2));

            var entity = RestoreToken(
                TokenStatus.Consumed,
                consumedAt: consumedAt
            );

            Assert.Equal(
                consumedAt,
                entity.ConsumedAt
            );

            Assert.Equal(
                consumedAt.Offset,
                entity.ConsumedAt!.Value.Offset
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Restore_ConsumedAtAtOrAfterExpiry_ThrowsInvariantViolation(long ticksFromExpiry)
        {
            var consumedAt = D.AddTicks(ticksFromExpiry);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestoreToken(
                    TokenStatus.Consumed,
                    consumedAt: consumedAt
                )
            );

            AssertInconsistentStatusInvariant(
                exception,
                TokenStatus.Consumed,
                D,
                consumedAt,
                null
            );
        }

        [Fact]
        public void Restore_ConsumedAtAtExpiryWithDifferentOffset_ThrowsInvariantViolation()
        {
            var consumedAt = D.ToOffset(TimeSpan.FromHours(-3));

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                RestoreToken(
                    TokenStatus.Consumed,
                    consumedAt: consumedAt
                )
            );

            AssertInconsistentStatusInvariant(
                exception,
                TokenStatus.Consumed,
                D,
                consumedAt,
                null
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(2)]
        public void Restore_RevokedAtOnOrAfterCreation_Succeeds(long ticksFromBoundary)
        {
            var revokedAt = ticksFromBoundary switch
            {
                0 => T0,
                -1 => D.AddTicks(-1),
                1 => D,
                _ => D.AddTicks(1)
            };

            var entity = RestoreToken(
                TokenStatus.Revoked,
                revokedAt: revokedAt
            );

            Assert.Equal(
                TokenStatus.Revoked,
                entity.TokenStatus
            );

            Assert.Equal(
                revokedAt,
                entity.RevokedAt
            );

            Assert.Null(entity.ConsumedAt);
        }

        [Fact]
        public void Restore_ValidationOrder_ReportsFirstViolation()
        {
            var emptyFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    null!, CreateUserId(), CreateRefreshToken(),
                    CreateDeviceInfo(), CreateIp(), null,
                    (TokenStatus)999, T0, null, T0, null
                )
            );

            AssertEmptyField(
                emptyFirst,
                "userRefreshTokenId",
                "Restore"
            );

            var statusFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    CreateTokenId(), CreateUserId(), CreateRefreshToken(),
                    CreateDeviceInfo(), CreateIp(), null,
                    (TokenStatus)999, T0, null, T0, null
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                statusFirst.ExceptionCode
            );

            var lifetimeFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRefreshToken.Restore(
                    CreateTokenId(), CreateUserId(), CreateRefreshToken(),
                    CreateDeviceInfo(), CreateIp(), null,
                    TokenStatus.Active, T0, T0.AddTicks(-1), T0, null
                )
            );

            AssertInvariant(
                lifetimeFirst,
                "Expiration time must be later than creation time.",
                "Restore"
            );

            var chronologyFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRefreshToken.Restore(
                    CreateTokenId(), CreateUserId(), CreateRefreshToken(),
                    CreateDeviceInfo(), CreateIp(), null,
                    TokenStatus.Active, D, T0.AddTicks(-1), T0, null
                )
            );

            AssertOutOfRangeField(
                chronologyFirst,
                "consumedAt",
                "createdAt",
                T0,
                "Restore"
            );
        }

        public static TheoryData<TokenStatus, long, bool> CanBeUsedCases()
        {
            var data = new TheoryData<TokenStatus, long, bool>();

            foreach (var status in new[]
                {
                    TokenStatus.Active,
                    TokenStatus.Expired,
                    TokenStatus.Consumed,
                    TokenStatus.Revoked
                })
            {
                for (var point = 0; point <= 4; point++)
                {
                    var expected = status == TokenStatus.Active
                                   && point is >= 1 and <= 2;

                    data.Add(status, point, expected);
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(CanBeUsedCases))]
        public void CanBeUsed_StatusAndTime_ReturnsExpectedResult(
            TokenStatus status,
            long point,
            bool expected)
        {
            DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
            DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

            var entity = RestoreToken(
                status,
                consumedAt,
                revokedAt
            );

            var now = point switch
            {
                0 => T0.AddTicks(-1),
                1 => T0,
                2 => D.AddTicks(-1),
                3 => D,
                _ => D.AddTicks(1)
            };

            CaptureAndAssertUnchanged(entity, () =>
                Assert.Equal(expected, entity.CanBeUsed(now)));
        }

        [Theory]
        [InlineData(TokenStatus.Active)]
        [InlineData(TokenStatus.Expired)]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        public void HasExpirationElapsed_RegardlessOfStatus_ComparesOnlyExpiry(TokenStatus status)
        {
            DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
            DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

            var entity = RestoreToken(
                status,
                consumedAt,
                revokedAt
            );

            CaptureAndAssertUnchanged(entity, () =>
            {
                Assert.False(entity.HasExpirationElapsed(T0.AddTicks(-1)));
                Assert.False(entity.HasExpirationElapsed(T0));
                Assert.False(entity.HasExpirationElapsed(D.AddTicks(-1)));
                Assert.True(entity.HasExpirationElapsed(D));
                Assert.True(entity.HasExpirationElapsed(D.AddTicks(1)));
            });
        }

        [Theory]
        [InlineData(TokenStatus.Active)]
        [InlineData(TokenStatus.Expired)]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        public void GetRemainingUsableTime_StatusAndTime_ReturnsExpectedDuration(TokenStatus status)
        {
            DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
            DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

            var entity = RestoreToken(
                status,
                consumedAt,
                revokedAt
            );

            CaptureAndAssertUnchanged(entity, () =>
            {
                Assert.Equal(
                    TimeSpan.Zero,
                    entity.GetRemainingUsableTime(T0.AddTicks(-1))
                );

                Assert.Equal(
                    status == TokenStatus.Active ? D - T0 : TimeSpan.Zero,
                    entity.GetRemainingUsableTime(T0)
                );

                Assert.Equal(
                    status == TokenStatus.Active ? TimeSpan.FromTicks(1) : TimeSpan.Zero,
                    entity.GetRemainingUsableTime(D.AddTicks(-1))
                );

                Assert.Equal(
                    TimeSpan.Zero,
                    entity.GetRemainingUsableTime(D)
                );

                Assert.Equal(
                    TimeSpan.Zero,
                    entity.GetRemainingUsableTime(D.AddTicks(1))
                );
            });
        }

        [Fact]
        public void Queries_DoNotChangeStateOrCollections()
        {
            foreach (var status in new[]
                     {
                        TokenStatus.Active,
                        TokenStatus.Expired,
                        TokenStatus.Consumed,
                        TokenStatus.Revoked
                     })
            {
                DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
                DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

                var entity = RestoreToken(
                    status,
                    consumedAt,
                    revokedAt
                );

                foreach (var now in new[]
                         {
                            T0.AddTicks(-1),
                            T0,
                            D.AddTicks(-1),
                            D,
                            D.AddTicks(1)
                         })
                {
                    CaptureAndAssertUnchanged(entity, () =>
                    {
                        _ = entity.CanBeUsed(now);
                        _ = entity.HasExpirationElapsed(now);
                        _ = entity.GetRemainingUsableTime(now);
                    });
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConsumeOrRevoke_ActiveWithinLifetime_Succeeds(bool useConsume)
        {
            foreach (var now in new[] { T0, Mid, D.AddTicks(-1) })
            {
                var entity = RestoreToken(TokenStatus.Active);
                var expectedId = entity.Id;
                var expectedUserId = entity.UserId;
                var expectedToken = entity.RefreshToken;
                var expectedDevice = entity.DeviceInfo;
                var expectedIp = entity.IpAddress;
                var expectedCity = entity.City;
                var expectedCreatedAt = entity.CreatedAt;
                var expectedExpiresAt = entity.ExpiresAt;

                var result = useConsume
                    ? entity.Consume(now)
                    : entity.Revoke(now);

                Assert.True(result.IsSuccess);
                Assert.Empty(result.Errors);

                Assert.Equal(
                    useConsume ? TokenStatus.Consumed : TokenStatus.Revoked,
                    entity.TokenStatus
                );

                Assert.Equal(
                    useConsume ? now : null,
                    entity.ConsumedAt
                );

                Assert.Equal(
                    useConsume ? null : now,
                    entity.RevokedAt
                );

                AssertImmutableProperties(
                    entity,
                    expectedId,
                    expectedUserId,
                    expectedToken,
                    expectedDevice,
                    expectedIp,
                    expectedCity,
                    expectedCreatedAt,
                    expectedExpiresAt
                );
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConsumeOrRevoke_BeforeCreation_ThrowsDataInconsistency(bool useConsume)
        {
            foreach (var status in new[]
                     {
                        TokenStatus.Active,
                        TokenStatus.Expired,
                        TokenStatus.Consumed,
                        TokenStatus.Revoked
                     })
            {
                DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
                DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

                var entity = RestoreToken(
                    status,
                    consumedAt,
                    revokedAt
                );

                var now = T0.AddTicks(-1);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    {
                        if (useConsume)
                        {
                            entity.Consume(now);
                        }
                        else
                        {
                            entity.Revoke(now);
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
        [InlineData(true, 0)]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void ConsumeOrRevoke_ActiveAtOrAfterExpiry_ReturnsExpiredFailureAndMarksExpired(
            bool useConsume,
            long ticksFromExpiry)
        {
            var entity = RestoreToken(TokenStatus.Active);
            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedToken = entity.RefreshToken;
            var expectedDevice = entity.DeviceInfo;
            var expectedIp = entity.IpAddress;
            var expectedCity = entity.City;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedExpiresAt = entity.ExpiresAt;
            var now = D.AddTicks(ticksFromExpiry);

            var result = useConsume
                ? entity.Consume(now)
                : entity.Revoke(now);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Forbidden,
                error.ErrorType
            );

            Assert.Equal(
                "TOKEN_STATUS.EXPIRED_TOKEN",
                error.Code
            );

            Assert.Equal(
                "Refresh token operation failed for type 'UserRefreshToken': " +
                "the token has expired and can no longer be used.",
                error.Message
            );

            Assert.Equal(
                TokenStatus.Expired,
                entity.TokenStatus
            );

            Assert.Null(entity.ConsumedAt);
            Assert.Null(entity.RevokedAt);

            AssertImmutableProperties(
                entity,
                expectedId,
                expectedUserId,
                expectedToken,
                expectedDevice,
                expectedIp,
                expectedCity,
                expectedCreatedAt,
                expectedExpiresAt
            );
        }

        [Theory]
        [InlineData(true, TokenStatus.Consumed)]
        [InlineData(true, TokenStatus.Revoked)]
        [InlineData(true, TokenStatus.Expired)]
        [InlineData(false, TokenStatus.Consumed)]
        [InlineData(false, TokenStatus.Revoked)]
        [InlineData(false, TokenStatus.Expired)]
        public void ConsumeOrRevoke_TerminalStatus_ReturnsConflictAndPreservesState(
            bool useConsume,
            TokenStatus status)
        {
            DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
            DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

            var entity = RestoreToken(
                status,
                consumedAt,
                revokedAt
            );

            var target = useConsume ? TokenStatus.Consumed : TokenStatus.Revoked;

            foreach (var now in new[] { T0, D.AddTicks(-1), D, D.AddTicks(1) })
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var result = useConsume
                        ? entity.Consume(now)
                        : entity.Revoke(now);

                    AssertInvalidStatusTransition(
                        result,
                        status,
                        target
                    );
                });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConsumeOrRevoke_RepeatedCall_ReturnsConflictAndKeepsOriginalTimestamp(bool useConsume)
        {
            var entity = RestoreToken(TokenStatus.Active);
            var target = useConsume ? TokenStatus.Consumed : TokenStatus.Revoked;

            var first = useConsume
                ? entity.Consume(Mid)
                : entity.Revoke(Mid);

            Assert.True(first.IsSuccess);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var second = useConsume
                    ? entity.Consume(Mid.AddHours(1))
                    : entity.Revoke(Mid.AddHours(1));

                AssertInvalidStatusTransition(
                    second,
                    target,
                    target
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConsumeOrRevoke_OtherTransitionAfterSuccess_ReturnsConflictAndPreservesState(bool consumeFirst)
        {
            var entity = RestoreToken(TokenStatus.Active);

            if (consumeFirst)
            {
                Assert.True(entity.Consume(Mid).IsSuccess);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var result = entity.Revoke(Mid.AddHours(1));

                    AssertInvalidStatusTransition(
                        result,
                        TokenStatus.Consumed,
                        TokenStatus.Revoked
                    );
                });
            }
            else
            {
                Assert.True(entity.Revoke(Mid).IsSuccess);

                CaptureAndAssertUnchanged(entity, () =>
                {
                    var result = entity.Consume(Mid.AddHours(1));

                    AssertInvalidStatusTransition(
                        result,
                        TokenStatus.Revoked,
                        TokenStatus.Consumed
                    );
                });
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(-1)]
        public void Terminate_ActiveWithinLifetime_RevokesToken(long daysOrTicksFlag)
        {
            var now = daysOrTicksFlag switch
            {
                0 => T0,
                3 => Mid,
                _ => D.AddTicks(-1)
            };
            var entity = RestoreToken(TokenStatus.Active);
            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedToken = entity.RefreshToken;
            var expectedDevice = entity.DeviceInfo;
            var expectedIp = entity.IpAddress;
            var expectedCity = entity.City;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedExpiresAt = entity.ExpiresAt;

            entity.Terminate(now);

            Assert.Equal(
                TokenStatus.Revoked,
                entity.TokenStatus
            );

            Assert.Equal(
                now,
                entity.RevokedAt
            );

            Assert.Null(entity.ConsumedAt);

            AssertImmutableProperties(
                entity,
                expectedId,
                expectedUserId,
                expectedToken,
                expectedDevice,
                expectedIp,
                expectedCity,
                expectedCreatedAt,
                expectedExpiresAt
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Terminate_ActiveAtOrAfterExpiry_MarksExpired(long ticksFromExpiry)
        {
            var entity = RestoreToken(TokenStatus.Active);
            var expectedId = entity.Id;
            var expectedUserId = entity.UserId;
            var expectedToken = entity.RefreshToken;
            var expectedDevice = entity.DeviceInfo;
            var expectedIp = entity.IpAddress;
            var expectedCity = entity.City;
            var expectedCreatedAt = entity.CreatedAt;
            var expectedExpiresAt = entity.ExpiresAt;
            var now = D.AddTicks(ticksFromExpiry);

            entity.Terminate(now);

            Assert.Equal(
                TokenStatus.Expired,
                entity.TokenStatus
            );

            Assert.Null(entity.ConsumedAt);
            Assert.Null(entity.RevokedAt);

            AssertImmutableProperties(
                entity,
                expectedId,
                expectedUserId,
                expectedToken,
                expectedDevice,
                expectedIp,
                expectedCity,
                expectedCreatedAt,
                expectedExpiresAt
            );
        }

        [Theory]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        [InlineData(TokenStatus.Expired)]
        public void Terminate_NonActiveStatus_ThrowsPreconditionFailed(TokenStatus status)
        {
            DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
            DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

            var entity = RestoreToken(
                status,
                consumedAt,
                revokedAt
            );

            foreach (var now in new[] { Mid, D.AddTicks(1) })
            {
                CaptureAndAssertUnchanged(entity, () =>
                {
                    var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                        entity.Terminate(now));

                    AssertTerminatePreconditionFailed(
                        exception,
                        entity,
                        status,
                        now
                    );
                });
            }
        }

        [Theory]
        [InlineData(TokenStatus.Active)]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        [InlineData(TokenStatus.Expired)]
        public void Terminate_BeforeCreation_ThrowsDataInconsistency(TokenStatus status)
        {
            DateTimeOffset? consumedAt = status == TokenStatus.Consumed ? Mid : null;
            DateTimeOffset? revokedAt = status == TokenStatus.Revoked ? Mid : null;

            var entity = RestoreToken(
                status,
                consumedAt,
                revokedAt
            );

            var now = T0.AddTicks(-1);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    entity.Terminate(now));

                AssertOutOfRangeField(
                    exception,
                    "now",
                    "CreatedAt",
                    T0,
                    "Update"
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Terminate_RepeatedCall_ThrowsPreconditionFailedAndPreservesState(bool expireFirst)
        {
            var entity = RestoreToken(TokenStatus.Active);
            var firstNow = expireFirst ? D : Mid;

            entity.Terminate(firstNow);

            var expectedStatus = expireFirst
                ? TokenStatus.Expired
                : TokenStatus.Revoked;

            DateTimeOffset? expectedRevoked = expireFirst
                ? null
                : firstNow;

            var secondNow = firstNow.AddHours(1);

            CaptureAndAssertUnchanged(entity, () =>
            {
                var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                    entity.Terminate(secondNow));

                AssertTerminatePreconditionFailed(
                    exception,
                    entity,
                    expectedStatus,
                    secondNow
                );
            });

            Assert.Equal(
                expectedStatus,
                entity.TokenStatus
            );

            Assert.Equal(
                expectedRevoked,
                entity.RevokedAt
            );

            Assert.Null(entity.ConsumedAt);
        }
    }
}
