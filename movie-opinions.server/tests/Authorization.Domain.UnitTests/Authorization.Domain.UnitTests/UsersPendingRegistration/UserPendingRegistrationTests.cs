using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.DomainEvents;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.UnitTests.UsersPendingRegistration
{
    public class UserPendingRegistrationTests
    {
        #region Test data
        // Фіксовані дати забезпечують відтворюваність сценаріїв строку дії.
        // Fixed timestamps make lifetime scenarios reproducible.
        private static readonly DateTimeOffset T0 = new(
            2026, 11, 3,
            12, 0, 0,
            TimeSpan.Zero
        );
        #endregion

        #region Helpers
        private static Login CreateEmailLogin()
            => Login.From(Email.Restore("serhii@example.com"));

        private static Login CreatePhoneLogin()
            => Login.From(
                Phone.Restore(
                    PhoneCountryCode.Restore("+380"),
                    PhoneNationalNumber.Restore("981234567")
                )
            );

        private static Password CreatePassword(string hashValue = "TEST.HASH.1")
            => Password.Create(PasswordHash.Create(hashValue));

        private static Password CreateOtherPassword()
            => CreatePassword("TEST.HASH.2");

        private static UserPendingRegistrationId CreateFixedId()
            => UserPendingRegistrationId.Restore(
                Guid.Parse("11111111-1111-4111-8111-111111111111")
            );

        private static RegistrationFlowToken CreateFixedToken(byte fill = 0)
        {
            var bytes = new byte[64];

            Array.Fill(
                bytes,
                fill
            );

            var value = Convert.ToBase64String(bytes);

            var result = RegistrationFlowToken.Parse(value);

            Assert.True(result.IsSuccess);

            return result.Value;
        }

        private static UserPendingRegistration RestoreAggregate(
            Login? login = null,
            Password? password = null,
            DateTimeOffset? createdAt = null,
            DateTimeOffset? expiresAt = null,
            UserPendingRegistrationId? id = null,
            RegistrationFlowToken? token = null)
        {
            var resolvedCreatedAt = createdAt ?? T0;

            return UserPendingRegistration.Restore(
                id ?? CreateFixedId(),
                login ?? CreateEmailLogin(),
                password ?? CreatePassword(),
                token ?? CreateFixedToken(),
                resolvedCreatedAt,
                expiresAt ?? resolvedCreatedAt.AddHours(1)
            );
        }

        private static void AssertEmptyValueException(
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
                "UserPendingRegistration",
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

        // Очікуваний стан передається зі знімка, зробленого до операції.
        // Expected state comes from a snapshot captured before the operation.
        private static void AssertUnchangedState(
            UserPendingRegistration aggregate,
            UserPendingRegistrationId expectedId,
            Login expectedLogin,
            Password expectedPassword,
            RegistrationFlowToken expectedToken,
            string expectedTokenValue,
            DateTimeOffset expectedCreatedAt,
            DateTimeOffset expectedExpiresAt,
            object[] expectedEvents)
        {
            Assert.Same(
                expectedId,
                aggregate.Id);

            Assert.Same(
                expectedLogin,
                aggregate.Login
            );

            Assert.Same(
                expectedPassword,
                aggregate.Password
            );

            Assert.Same(
                expectedToken,
                aggregate.RegistrationFlowToken
            );

            Assert.Equal(
                expectedTokenValue,
                aggregate.RegistrationFlowToken.Value
            );

            Assert.Equal(
                expectedCreatedAt,
                aggregate.CreatedAt
            );

            Assert.Equal(
                expectedExpiresAt,
                aggregate.ExpiresAt
            );

            Assert.Equal(
                expectedEvents.Length,
                aggregate.DomainEvents.Count
            );

            for (var i = 0; i < expectedEvents.Length; i++)
            {
                Assert.Same(
                    expectedEvents[i],
                    aggregate.DomainEvents[i]
                );
            }

            Assert.Empty(aggregate.AggregateChanges);
        }
        #endregion

        #region Create
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ValidArguments_SetsInitialState(bool useEmailLogin)
        {
            var login = useEmailLogin ? CreateEmailLogin() : CreatePhoneLogin();
            var password = CreatePassword();

            var aggregate = UserPendingRegistration.Create(
                login,
                password,
                T0
            );

            Assert.Same(
                login,
                aggregate.Login
            );

            Assert.Same(
                password,
                aggregate.Password
            );

            Assert.NotEqual(
                Guid.Empty,
                aggregate.Id.Value
            );

            Assert.True(RegistrationFlowToken.Parse(aggregate.RegistrationFlowToken.Value).IsSuccess);

            Assert.Equal(
                T0,
                aggregate.CreatedAt
            );

            Assert.Equal(
                T0.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.Empty(aggregate.AggregateChanges);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ValidArguments_RecordsRegistrationRequestedEvent(bool useEmailLogin)
        {
            var login = useEmailLogin ? CreateEmailLogin() : CreatePhoneLogin();
            var password = CreatePassword();

            var aggregate = UserPendingRegistration.Create(
                login,
                password,
                T0
            );

            var domainEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                Assert.Single(aggregate.DomainEvents)
            );

            Assert.Same(
                aggregate.Id,
                domainEvent.PendingRegistrationId
            );

            Assert.Same(
                login,
                domainEvent.Login
            );

            Assert.Equal(
                T0,
                domainEvent.OccurredOn
            );
        }

        [Fact]
        public void Create_NullLogin_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Create(
                    null!,
                    CreatePassword(),
                    T0
                )
            );

            AssertEmptyValueException(exception, "login", "Create");
        }

        [Fact]
        public void Create_NullPassword_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Create(
                    CreateEmailLogin(),
                    null!,
                    T0
                )
            );

            AssertEmptyValueException(exception, "password", "Create");
        }

        [Fact]
        public void Create_BothArgumentsNull_ReportsLogin()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Create(
                    null!,
                    null!,
                    T0
                )
            );

            AssertEmptyValueException(exception, "login", "Create");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(-5)]
        public void Create_ProvidedTime_PreservesTimestampAndOffset(int offsetHours)
        {
            var now = new DateTimeOffset(
                2026, 11, 3,
                12, 0, 0,
                TimeSpan.FromHours(offsetHours)
            );

            var aggregate = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                now
            );

            Assert.Equal(
                now,
                aggregate.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                aggregate.CreatedAt.Offset
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.Equal(
                now.Offset,
                aggregate.ExpiresAt.Offset
            );

            var domainEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                Assert.Single(aggregate.DomainEvents)
            );

            Assert.Equal(
                now,
                domainEvent.OccurredOn
            );

            Assert.Equal(
                now.Offset,
                domainEvent.OccurredOn.Offset
            );
        }

        [Fact]
        public void Create_SeparateCalls_GenerateDifferentIdsAndTokens()
        {
            var login = CreateEmailLogin();
            var password = CreatePassword();

            var first = UserPendingRegistration.Create(
                login,
                password,
                T0
            );

            var second = UserPendingRegistration.Create(
                login,
                password,
                T0
            );

            Assert.NotEqual(
                first.Id.Value,
                second.Id.Value
            );

            Assert.NotEqual(
                first.RegistrationFlowToken.Value,
                second.RegistrationFlowToken.Value
            );

            Assert.NotSame(
                Assert.Single(first.DomainEvents),
                Assert.Single(second.DomainEvents)
            );
        }
        #endregion

        #region Restore: stored state
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Restore_ValidArguments_PreservesStoredState(bool useEmailLogin)
        {
            var id = CreateFixedId();
            var login = useEmailLogin ? CreateEmailLogin() : CreatePhoneLogin();
            var password = CreatePassword();
            var token = CreateFixedToken();
            var createdAt = T0;
            var expiresAt = T0.AddHours(1);

            var aggregate = UserPendingRegistration.Restore(
                id,
                login,
                password,
                token,
                createdAt,
                expiresAt
            );

            Assert.Same(
                id,
                aggregate.Id
            );

            Assert.Same(
                login,
                aggregate.Login
            );

            Assert.Same(
                password,
                aggregate.Password
            );

            Assert.Same(
                token,
                aggregate.RegistrationFlowToken
            );

            Assert.Equal(
                createdAt,
                aggregate.CreatedAt
            );
            Assert.Equal(
                expiresAt,
                aggregate.ExpiresAt
            );
        }

        [Fact]
        public void Restore_ValidArguments_HasNoEventsOrAggregateChanges()
        {
            var aggregate = RestoreAggregate();

            Assert.Empty(aggregate.DomainEvents);
            Assert.Empty(aggregate.AggregateChanges);
        }
        #endregion

        #region Restore: required arguments
        [Fact]
        public void Restore_NullId_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Restore(
                    null!,
                    CreateEmailLogin(),
                    CreatePassword(),
                    CreateFixedToken(),
                    T0,
                    T0.AddHours(1)
                )
            );

            AssertEmptyValueException(
                exception,
                "userPendingRegistrationId",
                "Restore"
            );
        }

        [Fact]
        public void Restore_NullLogin_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    null!,
                    CreatePassword(),
                    CreateFixedToken(),
                    T0,
                    T0.AddHours(1)
                )
            );

            AssertEmptyValueException(
                exception,
                "login",
                "Restore"
            );
        }

        [Fact]
        public void Restore_NullPassword_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    CreateEmailLogin(),
                    null!,
                    CreateFixedToken(),
                    T0,
                    T0.AddHours(1)
                )
            );

            AssertEmptyValueException(
                exception,
                "password",
                "Restore"
            );
        }

        [Fact]
        public void Restore_NullFlowToken_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    CreateEmailLogin(),
                    CreatePassword(),
                    null!,
                    T0,
                    T0.AddHours(1)
                )
            );

            AssertEmptyValueException(
                exception,
                "registrationFlowToken",
                "Restore"
            );
        }

        [Theory]
        [InlineData(true, true, true, true, "userPendingRegistrationId")]
        [InlineData(false, true, true, true, "login")]
        [InlineData(false, false, true, true, "password")]
        public void Restore_MultipleNullArguments_ReportsFirstMissingArgument(
            bool nullId,
            bool nullLogin,
            bool nullPassword,
            bool nullToken,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Restore(
                    nullId ? null! : CreateFixedId(),
                    nullLogin ? null! : CreateEmailLogin(),
                    nullPassword ? null! : CreatePassword(),
                    nullToken ? null! : CreateFixedToken(),
                    T0,
                    T0.AddHours(1)
                )
            );

            AssertEmptyValueException(
                exception,
                expectedField,
                "Restore"
            );
        }
        #endregion

        #region Restore: lifetime and validation order
        [Theory]
        [InlineData(-TimeSpan.TicksPerMinute)]
        [InlineData(-1L)]
        [InlineData(0L)]
        public void Restore_ExpirationNotAfterCreation_ThrowsDomainInvariantViolationException(
            long ticksOffset)
        {
            var createdAt = T0;
            var expiresAt = createdAt.AddTicks(ticksOffset);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    CreateEmailLogin(),
                    CreatePassword(),
                    CreateFixedToken(),
                    createdAt,
                    expiresAt
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
                "UserPendingRegistration",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Expiration time must be later than creation time.",
                exception.Context["Rule"]
            );

            Assert.Equal(
                createdAt,
                exception.Context["State_CreatedAt"]
            );
            Assert.Equal(
                expiresAt,
                exception.Context["State_ExpiresAt"]
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30 * TimeSpan.TicksPerMinute)]
        [InlineData(TimeSpan.TicksPerHour)]
        [InlineData(2 * TimeSpan.TicksPerHour)]
        public void Restore_PositiveLifetime_PreservesProvidedExpiration(long durationTicks)
        {
            var createdAt = T0;
            var expiresAt = createdAt.AddTicks(durationTicks);

            var aggregate = UserPendingRegistration.Restore(
                CreateFixedId(),
                CreateEmailLogin(),
                CreatePassword(),
                CreateFixedToken(),
                createdAt,
                expiresAt
            );

            Assert.Equal(
                createdAt,
                aggregate.CreatedAt
            );
            Assert.Equal(
                expiresAt,
                aggregate.ExpiresAt
            );
        }

        // Restore зберігає історичний строк дії, навіть якщо перевірка вже показує спливання.
        // Restore preserves the stored lifetime even when an expiration check reports it elapsed.
        [Fact]
        public void Restore_ExpiredFlow_PreservesStoredState()
        {
            var createdAt = T0;
            var expiresAt = T0.AddHours(1);

            var aggregate = UserPendingRegistration.Restore(
                CreateFixedId(),
                CreateEmailLogin(),
                CreatePassword(),
                CreateFixedToken(),
                createdAt,
                expiresAt
            );

            Assert.True(aggregate.IsExpired(expiresAt.AddTicks(1)));

            Assert.Equal(
                createdAt,
                aggregate.CreatedAt
            );

            Assert.Equal(
                expiresAt,
                aggregate.ExpiresAt
            );

            Assert.Empty(
                aggregate.
                DomainEvents
            );
        }

        // Різні UTC-зсуви можуть представляти той самий момент часу.
        // Different UTC offsets can represent the same instant.
        [Fact]
        public void Restore_EqualInstantsWithDifferentOffsets_ThrowsDomainInvariantViolationException()
        {
            var createdAt = T0;
            var expiresAt = createdAt.ToOffset(TimeSpan.FromHours(2));

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    CreateEmailLogin(),
                    CreatePassword(),
                    CreateFixedToken(),
                    createdAt,
                    expiresAt
                )
            );

            Assert.Equal(
                "Expiration time must be later than creation time.",
                exception.Context["Rule"]
            );
        }

        [Fact]
        public void Restore_LaterInstantWithEarlierLocalTime_PreservesTimestamps()
        {
            var createdAt = new DateTimeOffset(
                2026, 11, 3,
                12, 0, 0,
                TimeSpan.FromHours(2)
            );
            var expiresAt = new DateTimeOffset(
                2026, 11, 3,
                11, 30, 0,
                TimeSpan.FromHours(-5)
            );

            Assert.True(expiresAt > createdAt);

            var aggregate = UserPendingRegistration.Restore(
                CreateFixedId(),
                CreateEmailLogin(),
                CreatePassword(),
                CreateFixedToken(),
                createdAt,
                expiresAt
            );

            Assert.Equal(
                createdAt,
                aggregate.CreatedAt
            );

            Assert.Equal(
                createdAt.Offset,
                aggregate.CreatedAt.Offset
            );

            Assert.Equal(
                expiresAt,
                aggregate.ExpiresAt
            );

            Assert.Equal(
                expiresAt.Offset,
                aggregate.ExpiresAt.Offset
            );
        }

        [Fact]
        public void Restore_EarlierInstantWithLaterLocalTime_ThrowsDomainInvariantViolationException()
        {
            var createdAt = new DateTimeOffset(
                2026, 11, 3,
                12, 0, 0,
                TimeSpan.FromHours(-5)
            );
            var expiresAt = new DateTimeOffset(
                2026, 11, 3,
                14, 0, 0,
                TimeSpan.FromHours(2)
            );

            Assert.True(expiresAt < createdAt);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    CreateEmailLogin(),
                    CreatePassword(),
                    CreateFixedToken(),
                    createdAt,
                    expiresAt
                )
            );

            Assert.Equal(
                "Expiration time must be later than creation time.",
                exception.Context["Rule"]
            );
        }

        [Fact]
        public void Restore_NullArgumentAndInvalidDates_ReportsNullArgument()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingRegistration.Restore(
                    CreateFixedId(),
                    CreateEmailLogin(),
                    null!,
                    CreateFixedToken(),
                    T0,
                    T0
                )
            );

            AssertEmptyValueException(
                exception,
                "password",
                "Restore"
            );
        }
        #endregion

        #region Refresh: state and event
        [Fact]
        public void Refresh_ValidArguments_RenewsFlowAndPreservesIdentity()
        {
            var aggregate = RestoreAggregate();
            var originalId = aggregate.Id;
            var originalLogin = aggregate.Login;
            var originalCreatedAt = aggregate.CreatedAt;
            var originalTokenValue = aggregate.RegistrationFlowToken.Value;
            var newPassword = CreateOtherPassword();
            var now = T0.AddMinutes(10);

            aggregate.Refresh(
                newPassword,
                now
            );

            Assert.Same(
                originalId,
                aggregate.Id
            );

            Assert.Same(
                originalLogin,
                aggregate.Login
            );

            Assert.Equal(
                originalCreatedAt,
                aggregate.CreatedAt
            );

            Assert.Same(
                newPassword,
                aggregate.Password
            );

            Assert.NotEqual(
                originalTokenValue,
                aggregate.RegistrationFlowToken.Value
            );

            Assert.True(
                RegistrationFlowToken.Parse(aggregate.RegistrationFlowToken.Value).IsSuccess
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.Empty(aggregate.AggregateChanges);
        }

        [Fact]
        public void Refresh_ValidArguments_RecordsRegistrationRequestedEvent()
        {
            var aggregate = RestoreAggregate();
            var now = T0.AddMinutes(10);

            aggregate.Refresh(
                CreateOtherPassword(),
                now
            );

            var domainEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                Assert.Single(aggregate.DomainEvents)
            );

            Assert.Same(
                aggregate.Id,
                domainEvent.PendingRegistrationId
            );

            Assert.Same(
                aggregate.Login,
                domainEvent.Login
            );

            Assert.Equal(
                now,
                domainEvent.OccurredOn
            );
        }
        #endregion

        #region Refresh: invalid arguments
        [Fact]
        public void Refresh_NullPassword_ThrowsWithoutChangingState()
        {
            var aggregate = RestoreAggregate();
            aggregate.Refresh(
                CreateOtherPassword(),
                T0.AddMinutes(5)
            );

            var expectedId = aggregate.Id;
            var expectedLogin = aggregate.Login;
            var expectedPassword = aggregate.Password;
            var expectedToken = aggregate.RegistrationFlowToken;
            var expectedTokenValue = expectedToken.Value;
            var expectedCreatedAt = aggregate.CreatedAt;
            var expectedExpiresAt = aggregate.ExpiresAt;
            var expectedEvents = aggregate.DomainEvents.Cast<object>().ToArray();

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                aggregate.Refresh(
                    null!,
                    T0.AddMinutes(10)
                )
            );

            AssertEmptyValueException(
                exception,
                "password",
                "Update"
            );

            AssertUnchangedState(
                aggregate,
                expectedId,
                expectedLogin,
                expectedPassword,
                expectedToken,
                expectedTokenValue,
                expectedCreatedAt,
                expectedExpiresAt,
                expectedEvents
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-TimeSpan.TicksPerMinute)]
        public void Refresh_TimeBeforeCreation_ThrowsWithoutChangingState(long ticksBefore)
        {
            var aggregate = RestoreAggregate();
            aggregate.Refresh(
                CreateOtherPassword(),
                T0.AddMinutes(5)
            );

            var expectedId = aggregate.Id;
            var expectedLogin = aggregate.Login;
            var expectedPassword = aggregate.Password;
            var expectedToken = aggregate.RegistrationFlowToken;
            var expectedTokenValue = expectedToken.Value;
            var expectedCreatedAt = aggregate.CreatedAt;
            var expectedExpiresAt = aggregate.ExpiresAt;
            var expectedEvents = aggregate.DomainEvents.Cast<object>().ToArray();
            var earlier = T0.AddTicks(ticksBefore);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                aggregate.Refresh(
                    CreatePassword("TEST.HASH.3"),
                    earlier
                )
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
                "UserPendingRegistration",
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

            AssertUnchangedState(
                aggregate,
                expectedId,
                expectedLogin,
                expectedPassword,
                expectedToken,
                expectedTokenValue,
                expectedCreatedAt,
                expectedExpiresAt,
                expectedEvents
            );
        }

        [Fact]
        public void Refresh_NullPasswordAndEarlierTime_ReportsPassword()
        {
            var aggregate = RestoreAggregate();
            var expectedPassword = aggregate.Password;
            var expectedTokenValue = aggregate.RegistrationFlowToken.Value;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                aggregate.Refresh(
                    null!,
                    T0.AddMinutes(-1)
                )
            );

            AssertEmptyValueException(
                exception,
                "password",
                "Update"
            );

            Assert.Same(
                expectedPassword,
                aggregate.Password
            );

            Assert.Equal(
                expectedTokenValue,
                aggregate.RegistrationFlowToken.Value
            );
        }
        #endregion

        #region Refresh: timestamps and renewal
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(-5)]
        public void Refresh_AtCreationInstant_IsAllowed(int offsetHours)
        {
            var createdAt = T0;
            var aggregate = RestoreAggregate(createdAt: createdAt);
            var now = createdAt.ToOffset(TimeSpan.FromHours(offsetHours));

            aggregate.Refresh(
                CreateOtherPassword(),
                now
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.Single(aggregate.DomainEvents);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void Refresh_BeforeAtAndAfterExpiration_RenewsFlow(long ticksFromExpiration)
        {
            var aggregate = RestoreAggregate();
            var now = aggregate.ExpiresAt.AddTicks(ticksFromExpiration);
            var newPassword = CreateOtherPassword();
            var originalTokenValue = aggregate.RegistrationFlowToken.Value;

            aggregate.Refresh(
                newPassword,
                now
            );

            Assert.Same(
                newPassword,
                aggregate.Password
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.NotEqual(
                originalTokenValue,
                aggregate.RegistrationFlowToken.Value
            );

            var domainEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                Assert.Single(aggregate.DomainEvents)
            );

            Assert.Same(
                aggregate.Id,
                domainEvent.PendingRegistrationId
            );

            Assert.Same(
                aggregate.Login,
                domainEvent.Login
            );

            Assert.Equal(
                now,
                domainEvent.OccurredOn
            );
        }

        [Fact]
        public void Refresh_SamePasswordInstance_RenewsFlow()
        {
            var password = CreatePassword();
            var aggregate = RestoreAggregate(password: password);
            var originalTokenValue = aggregate.RegistrationFlowToken.Value;
            var now = T0.AddMinutes(10);

            aggregate.Refresh(
                password,
                now
            );

            Assert.Same(
                password,
                aggregate.Password
            );

            Assert.NotEqual(
                originalTokenValue,
                aggregate.RegistrationFlowToken.Value
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.Single(aggregate.DomainEvents);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(-5)]
        public void Refresh_ProvidedTime_PreservesTimestampAndOffset(int offsetHours)
        {
            var aggregate = RestoreAggregate();
            var originalCreatedAt = aggregate.CreatedAt;
            var now = T0.AddMinutes(15).ToOffset(TimeSpan.FromHours(offsetHours));

            aggregate.Refresh(
                CreateOtherPassword(),
                now
            );

            Assert.Equal(
                originalCreatedAt,
                aggregate.CreatedAt
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.Equal(
                now.Offset,
                aggregate.ExpiresAt.Offset
            );

            var domainEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                Assert.Single(aggregate.DomainEvents)
            );

            Assert.Equal(
                now,
                domainEvent.OccurredOn
            );

            Assert.Equal(
                now.Offset,
                domainEvent.OccurredOn.Offset
            );
        }
        #endregion

        #region Refresh: repeated calls and events
        [Fact]
        public void Refresh_ExistingEvents_AppendsWithoutRemovingThem()
        {
            var aggregate = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );
            var firstEvent = Assert.Single(aggregate.DomainEvents);

            aggregate.Refresh(
                CreateOtherPassword(),
                T0.AddMinutes(10)
            );

            Assert.Equal(
                2,
                aggregate.DomainEvents.Count
            );

            Assert.Same(
                firstEvent,
                aggregate.DomainEvents[0]
            );

            Assert.IsType<UserRegistrationRequestedEvent>(aggregate.DomainEvents[1]);
        }

        [Fact]
        public void Refresh_RepeatedCalls_UseLatestStateAndRecordEachEvent()
        {
            var aggregate = RestoreAggregate();
            var firstPassword = CreatePassword("TEST.HASH.A");
            var secondPassword = CreatePassword("TEST.HASH.B");
            var firstNow = T0.AddMinutes(10);
            var secondNow = T0.AddMinutes(20);

            aggregate.Refresh(
                firstPassword,
                firstNow
            );

            var tokenAfterFirst = aggregate.RegistrationFlowToken.Value;

            aggregate.Refresh(
                secondPassword,
                secondNow
            );

            Assert.Same(
                secondPassword,
                aggregate.Password
            );

            Assert.Equal(
                secondNow.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.NotEqual(
                tokenAfterFirst,
                aggregate.RegistrationFlowToken.Value
            );

            Assert.Equal(
                2,
                aggregate.DomainEvents.Count
            );

            var firstEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                aggregate.DomainEvents[0]
            );

            var secondEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                aggregate.DomainEvents[1]
            );

            Assert.Equal(
                firstNow,
                firstEvent.OccurredOn
            );

            Assert.Equal(
                secondNow,
                secondEvent.OccurredOn
            );
        }

        [Fact]
        public void Refresh_RepeatedCallsAtSameTime_RotateTokenAndRecordEachEvent()
        {
            var aggregate = RestoreAggregate();
            var firstPassword = CreatePassword("TEST.HASH.A");
            var secondPassword = CreatePassword("TEST.HASH.B");
            var now = T0.AddMinutes(10);

            aggregate.Refresh(
                firstPassword,
                now
            );

            var tokenAfterFirst = aggregate.RegistrationFlowToken.Value;

            aggregate.Refresh(
                secondPassword,
                now
            );

            Assert.Same(
                secondPassword,
                aggregate.Password
            );

            Assert.Equal(
                now.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.NotEqual(
                tokenAfterFirst,
                aggregate.RegistrationFlowToken.Value
            );

            Assert.Equal(
                2,
                aggregate.DomainEvents.Count
            );
        }

        // Межа тут — CreatedAt: переданий час може передувати попередньому Refresh.
        // The boundary here is CreatedAt: the supplied time may precede the previous Refresh.
        [Fact]
        public void Refresh_TimeBeforePreviousRefreshButNotBeforeCreation_UsesProvidedTime()
        {
            var aggregate = RestoreAggregate(createdAt: T0);
            var firstPassword = CreatePassword("TEST.HASH.A");
            var secondPassword = CreatePassword("TEST.HASH.B");
            var firstNow = T0.AddMinutes(20);
            var secondNow = T0.AddMinutes(10);

            aggregate.Refresh(
                firstPassword,
                firstNow
            );

            var tokenAfterFirst = aggregate.RegistrationFlowToken.Value;

            aggregate.Refresh(
                secondPassword,
                secondNow
            );

            Assert.Same(
                secondPassword,
                aggregate.Password
            );

            Assert.Equal(
                secondNow.AddHours(1),
                aggregate.ExpiresAt
            );

            Assert.NotEqual(
                tokenAfterFirst,
                aggregate.RegistrationFlowToken.Value
            );

            Assert.Equal(
                2,
                aggregate.DomainEvents.Count
            );

            var firstEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                aggregate.DomainEvents[0]
            );

            var secondEvent = Assert.IsType<UserRegistrationRequestedEvent>(
                aggregate.DomainEvents[1]
            );

            Assert.Equal(
                firstNow,
                firstEvent.OccurredOn
            );

            Assert.Equal(
                secondNow,
                secondEvent.OccurredOn
            );
        }
        #endregion

        #region IsExpired
        [Theory]
        [InlineData(-TimeSpan.TicksPerHour)]
        [InlineData(0)]
        [InlineData(-TimeSpan.TicksPerMinute)]
        [InlineData(-1)]
        public void IsExpired_BeforeExpiration_ReturnsFalse(long ticksFromExpiration)
        {
            var aggregate = RestoreAggregate();

            var pointInTime = ticksFromExpiration switch
            {
                0 => aggregate.CreatedAt,
                -TimeSpan.TicksPerHour => aggregate.CreatedAt.AddMinutes(-1),
                -TimeSpan.TicksPerMinute => aggregate.ExpiresAt.AddMinutes(-1),
                _ => aggregate.ExpiresAt.AddTicks(-1)
            };

            Assert.False(aggregate.IsExpired(pointInTime));
        }

        [Fact]
        public void IsExpired_AtExpiration_ReturnsTrue()
        {
            var aggregate = RestoreAggregate();

            Assert.True(aggregate.IsExpired(aggregate.ExpiresAt));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(TimeSpan.TicksPerMinute)]
        [InlineData(TimeSpan.TicksPerDay)]
        public void IsExpired_AfterExpiration_ReturnsTrue(long ticksAfter)
        {
            var aggregate = RestoreAggregate();

            Assert.True(aggregate.IsExpired(aggregate.ExpiresAt.AddTicks(ticksAfter)));
        }

        [Fact]
        public void IsExpired_DifferentOffsets_ComparesActualInstants()
        {
            var aggregate = RestoreAggregate();

            Assert.False(
                aggregate.IsExpired(
                    aggregate.ExpiresAt.AddTicks(-1).ToOffset(TimeSpan.FromHours(2))
                )
            );

            Assert.True(
                aggregate.IsExpired(
                    aggregate.ExpiresAt.ToOffset(TimeSpan.FromHours(-5))
                )
            );

            Assert.True(
                aggregate.IsExpired(
                    aggregate.ExpiresAt.AddTicks(1).ToOffset(TimeSpan.FromHours(-5))
                )
            );
        }

        [Fact]
        public void IsExpired_RepeatedQueries_DoNotChangeState()
        {
            var aggregate = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );

            var expectedId = aggregate.Id;
            var expectedLogin = aggregate.Login;
            var expectedPassword = aggregate.Password;
            var expectedToken = aggregate.RegistrationFlowToken;
            var expectedTokenValue = expectedToken.Value;
            var expectedCreatedAt = aggregate.CreatedAt;
            var expectedExpiresAt = aggregate.ExpiresAt;
            var expectedEvents = aggregate.DomainEvents.Cast<object>().ToArray();

            Assert.False(aggregate.IsExpired(expectedExpiresAt.AddTicks(-1)));
            Assert.True(aggregate.IsExpired(expectedExpiresAt));
            Assert.False(aggregate.IsExpired(expectedExpiresAt.AddTicks(-1)));

            AssertUnchangedState(
                aggregate,
                expectedId,
                expectedLogin,
                expectedPassword,
                expectedToken,
                expectedTokenValue,
                expectedCreatedAt,
                expectedExpiresAt,
                expectedEvents
            );
        }

        [Fact]
        public void IsExpired_AfterRefresh_UsesNewExpiration()
        {
            var aggregate = RestoreAggregate(
                createdAt: T0,
                expiresAt: T0.AddHours(1)
            );

            aggregate.Refresh(
                CreateOtherPassword(),
                T0.AddMinutes(30)
            );

            Assert.False(aggregate.IsExpired(T0.AddHours(1)));
            Assert.True(aggregate.IsExpired(T0.AddMinutes(30).AddHours(1)));
        }
        #endregion

        #region Restoration round trips
        [Fact]
        public void CreateThenRestore_PreservesStateWithoutRecordingEvents()
        {
            var created = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );

            var restored = UserPendingRegistration.Restore(
                created.Id,
                created.Login,
                created.Password,
                created.RegistrationFlowToken,
                created.CreatedAt,
                created.ExpiresAt
            );

            Assert.Equal(
                created.Id.Value,
                restored.Id.Value
            );

            Assert.Same(
                created.Login,
                restored.Login
            );

            Assert.Same(
                created.Password,
                restored.Password
            );

            Assert.Same(
                created.RegistrationFlowToken,
                restored.RegistrationFlowToken
            );

            Assert.Equal(
                created.CreatedAt,
                restored.CreatedAt
            );

            Assert.Equal(
                created.ExpiresAt,
                restored.ExpiresAt
            );

            Assert.Empty(restored.DomainEvents);
            Assert.Empty(restored.AggregateChanges);
            Assert.Single(created.DomainEvents);
        }

        [Fact]
        public void RefreshThenRestore_PreservesLatestFlowWithoutRecordingEvents()
        {
            var created = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );
            var refreshedPassword = CreateOtherPassword();
            var refreshAt = T0.AddMinutes(10);

            created.Refresh(
                refreshedPassword,
                refreshAt
            );

            var restored = UserPendingRegistration.Restore(
                created.Id,
                created.Login,
                created.Password,
                created.RegistrationFlowToken,
                created.CreatedAt,
                created.ExpiresAt
            );

            Assert.Same(
                refreshedPassword,
                restored.Password
            );

            Assert.Equal(
                created.RegistrationFlowToken.Value,
                restored.RegistrationFlowToken.Value
            );

            Assert.Equal(
                T0,
                restored.CreatedAt
            );

            Assert.Equal(
                refreshAt.AddHours(1),
                restored.ExpiresAt
            );

            Assert.Empty(restored.DomainEvents);
        }
        #endregion

        #region Timestamp limits
        [Fact]
        public void Create_BoundaryTimestampsWithinRange_Succeeds()
        {
            var min = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                DateTimeOffset.MinValue
            );
            var nearMaxTime = DateTimeOffset.MaxValue.AddHours(-1);

            var nearMax = UserPendingRegistration.Create(
                CreateEmailLogin(),
                CreatePassword(),
                nearMaxTime
            );

            Assert.Equal(
                DateTimeOffset.MinValue,
                min.CreatedAt
            );

            Assert.Equal(
                DateTimeOffset.MinValue.AddHours(1),
                min.ExpiresAt
            );

            Assert.Equal(
                nearMaxTime,
                nearMax.CreatedAt
            );

            Assert.Equal(
                DateTimeOffset.MaxValue,
                nearMax.ExpiresAt
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        public void Create_ExpirationWouldOverflow_ThrowsArgumentOutOfRangeException(
            int ticksFromLimit)
        {
            var now = ticksFromLimit == 0
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.MaxValue.AddHours(-1).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UserPendingRegistration.Create(
                    CreateEmailLogin(),
                    CreatePassword(),
                    now
                )
            );
        }

        [Fact]
        public void Refresh_ExpirationWouldOverflow_ThrowsArgumentOutOfRangeException()
        {
            var aggregate = RestoreAggregate();
            var overflowTime = DateTimeOffset.MaxValue.AddHours(-1).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                aggregate.Refresh(CreateOtherPassword(), overflowTime)
            );
        }
        #endregion
    }
}
