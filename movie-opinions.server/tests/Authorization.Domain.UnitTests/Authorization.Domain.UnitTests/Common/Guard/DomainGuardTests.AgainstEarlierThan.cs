using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Guard
{
    public partial class DomainGuardTests
    {
        private static readonly DateTimeOffset Minimum = new(
            2026, 10, 30,
            12, 0, 0,
            TimeSpan.Zero
        );

        [Fact]
        public void AgainstEarlierThan_ActualAfterMinimum_DoesNotThrow()
        {
            var actual = Minimum.AddMinutes(1);

            DomainGuard.AgainstEarlierThan<Error>(
                OperationType.Update,
                (actual, "UpdatedAt"),
                (Minimum, "CreatedAt")
            );
        }

        [Fact]
        public void AgainstEarlierThan_ActualEqualsMinimum_DoesNotThrow()
        {
            DomainGuard.AgainstEarlierThan<Error>(
                OperationType.Update,
                (Minimum, "UpdatedAt"),
                (Minimum, "CreatedAt")
            );
        }

        [Fact]
        public void AgainstEarlierThan_ActualOneTickBeforeMinimum_ThrowsDomainDataInconsistencyException()
        {
            var actual = Minimum.AddTicks(-1);

            Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstEarlierThan<Error>(
                    OperationType.Update,
                    (actual, "UpdatedAt"),
                    (Minimum, "CreatedAt")
                )
            );
        }

        [Fact]
        public void AgainstEarlierThan_ActualBeforeMinimum_ThrowsDomainDataInconsistencyException()
        {
            var actual = Minimum.AddMinutes(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstEarlierThan<Error>(
                    OperationType.Update,
                    (actual, "UpdatedAt"),
                    (Minimum, "CreatedAt")
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
                "UpdatedAt",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Error",
                exception.Context["Type"]
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
                Minimum,
                exception.Context["Custom_MinimumValue"]
            );

            Assert.IsType<DateTimeOffset>(exception.Context["Custom_MinimumValue"]);
        }

        [Theory]
        [InlineData(OperationType.Restore)]
        [InlineData(OperationType.Create)]
        [InlineData(OperationType.Update)]
        [InlineData(OperationType.Delete)]
        [InlineData(OperationType.Read)]
        [InlineData(OperationType.Compare)]
        public void AgainstEarlierThan_ExplicitOperation_IsUsedInExceptionContext(OperationType operationType)
        {
            var actual = Minimum.AddMinutes(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstEarlierThan<Error>(
                    operationType,
                    (actual, "UpdatedAt"),
                    (Minimum, "CreatedAt")
                )
            );

            Assert.Equal(
                operationType.ToString(),
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void AgainstEarlierThan_OwnerType_IsUsedInExceptionContext()
        {
            var actual = Minimum.AddMinutes(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstEarlierThan<Email>(
                    OperationType.Update,
                    (actual, "UpdatedAt"),
                    (Minimum, "CreatedAt")
                )
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );
        }

        [Theory]
        [InlineData("UpdatedAt", "CreatedAt")]
        [InlineData("Now", "LastLoginAt")]
        [InlineData("AttemptedAt", "MinimumAllowedAt")]
        public void AgainstEarlierThan_FieldNames_AreUsedInExceptionContext(
            string actualFieldName,
            string minimumFieldName)
        {
            var actual = Minimum.AddMinutes(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstEarlierThan<Error>(
                    OperationType.Update,
                    (actual, actualFieldName),
                    (Minimum, minimumFieldName)
                )
            );

            Assert.Equal(
                actualFieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                minimumFieldName,
                exception.Context["Custom_MinimumField"]
            );
        }

        [Fact]
        public void AgainstEarlierThan_SameInstantWithDifferentOffsets_DoesNotThrow()
        {
            var actual = Minimum.ToOffset(TimeSpan.FromHours(2));

            DomainGuard.AgainstEarlierThan<Error>(
                OperationType.Update,
                (actual, "UpdatedAt"),
                (Minimum, "CreatedAt")
            );
        }

        [Fact]
        public void AgainstEarlierThan_LaterInstantWithEarlierLocalTime_DoesNotThrow()
        {
            var actual = Minimum
                .AddMinutes(30)
                .ToOffset(TimeSpan.FromHours(-1));

            DomainGuard.AgainstEarlierThan<Error>(
                OperationType.Update,
                (actual, "UpdatedAt"),
                (Minimum, "CreatedAt")
            );
        }

        [Fact]
        public void AgainstEarlierThan_EarlierInstantWithLaterLocalTime_ThrowsDomainDataInconsistencyException()
        {
            var actual = Minimum
                .AddMinutes(-30)
                .ToOffset(TimeSpan.FromHours(2));

            Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstEarlierThan<Error>(
                    OperationType.Update,
                    (actual, "UpdatedAt"),
                    (Minimum, "CreatedAt")
                )
            );
        }
    }
}
