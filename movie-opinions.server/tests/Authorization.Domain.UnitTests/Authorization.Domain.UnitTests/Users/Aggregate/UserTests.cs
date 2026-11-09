using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        private static readonly DateTimeOffset T0 = new(
            2026, 11, 5,
            12, 0, 0,
            TimeSpan.Zero
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

        private static Password CreatePassword(string hashValue = "TEST.HASH.1")
            => Password.Create(PasswordHash.Create(hashValue));

        private static void AssertEmptyValueException(
            DomainDataInconsistencyException exception,
            string field)
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
                "User",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ValidArguments_SetsInitialState(bool useEmailLogin)
        {
            var login = useEmailLogin ? CreateEmailLogin() : CreatePhoneLogin();
            var password = CreatePassword();

            var user = User.Create(
                login,
                password,
                T0
            );

            Assert.NotNull(user.Id);

            Assert.NotEqual(
                Guid.Empty,
                user.Id.Value
            );

            Assert.Equal(
                7,
                user.Id.Value.Version
            );

            Assert.Same(
                login,
                user.Login
            );

            Assert.Same(
                password,
                user.Password
            );

            Assert.Equal(
                Role.User,
                user.Role
            );

            Assert.Equal(
                T0,
                user.CreatedAt
            );

            Assert.Null(user.UpdatedAt);
            Assert.Null(user.LastLoginAt);

            Assert.Equal(
                0,
                user.FailedPasswordAttempts
            );

            Assert.False(user.IsDeleted);
            Assert.False(user.IsAction);
            Assert.NotNull(user.RefreshTokens);
            Assert.Empty(user.RefreshTokens);
            Assert.NotNull(user.Restrictions);
            Assert.Empty(user.Restrictions);
            Assert.NotNull(user.RestrictionSessions);
            Assert.Empty(user.RestrictionSessions);
            Assert.NotNull(user.AggregateChanges);
            Assert.Empty(user.AggregateChanges);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ValidArguments_RecordsUserRegisteredEvent(bool useEmailLogin)
        {
            var login = useEmailLogin ? CreateEmailLogin() : CreatePhoneLogin();
            var password = CreatePassword();

            var user = User.Create(
                login,
                password,
                T0
            );

            var domainEvent = Assert.IsType<UserRegisteredEvent>(
                Assert.Single(user.DomainEvents)
            );

            Assert.Same(
                user.Id,
                domainEvent.UserId
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
                User.Create(
                    null!,
                    CreatePassword(),
                    T0
                )
            );

            AssertEmptyValueException(
                exception,
                "login"
            );
        }

        [Fact]
        public void Create_NullPassword_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                User.Create(
                    CreateEmailLogin(),
                    null!,
                    T0
                )
            );

            AssertEmptyValueException(
                exception,
                "password"
            );
        }

        [Fact]
        public void Create_BothArgumentsNull_ReportsLogin()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                User.Create(
                    null!,
                    null!,
                    T0
                )
            );

            AssertEmptyValueException(
                exception,
                "login"
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(-5)]
        public void Create_ProvidedTime_PreservesTimestampAndOffset(int offsetHours)
        {
            var now = new DateTimeOffset(
                2026, 11, 5,
                12, 0, 0,
                TimeSpan.FromHours(offsetHours)
            );

            var user = User.Create(
                CreateEmailLogin(),
                CreatePassword(),
                now
            );

            Assert.Equal(
                now,
                user.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                user.CreatedAt.Offset
            );

            var domainEvent = Assert.IsType<UserRegisteredEvent>(
                Assert.Single(user.DomainEvents)
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_BoundaryTimestamps_PreservesProvidedTime(bool useMinValue)
        {
            var now = useMinValue
                ? DateTimeOffset.MinValue
                : DateTimeOffset.MaxValue;

            var user = User.Create(
                CreateEmailLogin(),
                CreatePassword(),
                now
            );

            Assert.Equal(now, user.CreatedAt);

            var domainEvent = Assert.IsType<UserRegisteredEvent>(
                Assert.Single(user.DomainEvents)
            );

            Assert.Equal(
                now,
                domainEvent.OccurredOn
            );
        }

        [Fact]
        public void Create_SeparateCalls_CreateIndependentUsers()
        {
            var login = CreateEmailLogin();
            var password = CreatePassword();

            var first = User.Create(
                login,
                password,
                T0
            );

            var second = User.Create(
                login,
                password,
                T0
            );

            Assert.NotSame(
                first,
                second
            );

            Assert.NotEqual(
                first.Id.Value,
                second.Id.Value
            );

            var firstEvent = Assert.IsType<UserRegisteredEvent>(
                Assert.Single(first.DomainEvents)
            );

            var secondEvent = Assert.IsType<UserRegisteredEvent>(
                Assert.Single(second.DomainEvents)
            );

            Assert.Same(
                first.Id,
                firstEvent.UserId
            );

            Assert.Same(
                second.Id,
                secondEvent.UserId
            );

            first.ClearDomainEvents();

            Assert.Empty(first.DomainEvents);
            Assert.Single(second.DomainEvents);

            Assert.Same(
                secondEvent,
                second.DomainEvents[0]
            );
        }

        [Fact]
        public void RefreshTokens_NewUser_RejectsExternalMutation()
        {
            var user = User.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<UserRefreshToken>)user.RefreshTokens).Clear()
            );

            Assert.Empty(user.RefreshTokens);
        }

        [Fact]
        public void Restrictions_NewUser_RejectsExternalMutation()
        {
            var user = User.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<UserRestriction>)user.Restrictions).Clear()
            );

            Assert.Empty(user.Restrictions);
        }

        [Fact]
        public void RestrictionSessions_NewUser_RejectsExternalMutation()
        {
            var user = User.Create(
                CreateEmailLogin(),
                CreatePassword(),
                T0
            );

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<UserRestrictionSession>)user.RestrictionSessions).Clear()
            );

            Assert.Empty(user.RestrictionSessions);
        }
    }
}
