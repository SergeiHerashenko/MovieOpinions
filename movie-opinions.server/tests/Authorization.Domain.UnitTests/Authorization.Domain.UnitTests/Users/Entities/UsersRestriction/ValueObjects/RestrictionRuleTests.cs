using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRestriction.ValueObjects
{
    public class RestrictionRuleTests
    {
        #region Test data
        private const string ValidName = "LoginAttemptLimit";
        private const int ValidDuration = 60;
        #endregion

        #region Helpers: successful results
        private static void AssertCreateSuccess(
            string name,
            int durationMinutes)
        {
            var result = RestrictionRule.Create(
                name,
                durationMinutes
            );

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                name,
                result.Value.Name
            );

            Assert.Equal(
                durationMinutes,
                result.Value.DurationMinutes
            );
        }

        private static void AssertRestoreSuccess(
            string name,
            int durationMinutes)
        {
            var rule = RestrictionRule.Restore(
                name,
                durationMinutes
            );

            Assert.Equal(
                name,
                rule.Name
            );

            Assert.Equal(
                durationMinutes,
                rule.DurationMinutes
            );
        }
        #endregion

        #region Helpers: Create failures
        private static void AssertCreateEmptyName(
            string? name,
            int durationMinutes)
        {
            var result = RestrictionRule.Create(
                name!,
                durationMinutes
            );

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "RESTRICTION.EMPTY_NAME_RESTRICTION",
                error.Code
            );

            Assert.Equal(
                "Restriction rule validation failed for type 'RestrictionRule': " +
                "the rule name is empty or missing.",
                error.Message
            );
        }

        private static void AssertCreateInvalidDuration(
            string name,
            int durationMinutes)
        {
            var result = RestrictionRule.Create(
                name,
                durationMinutes
            );

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "RESTRICTION.INVALID_NUMBER_MINUTES",
                error.Code
            );

            Assert.Equal(
                "Restriction rule validation failed for type 'RestrictionRule': " +
                "duration must be greater than zero minutes, but the actual value is " +
                $"{durationMinutes}.",
                error.Message
            );
        }
        #endregion

        #region Helpers: Restore exceptions
        // Для збережених некоректних даних перевіряємо виняток і діагностичний контекст.
        // Invalid stored data is checked through its exception and diagnostic context.
        private static void AssertRestoreEmptyName(
            string? name,
            int durationMinutes)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestrictionRule.Restore(
                    name!,
                    durationMinutes
                )
            );

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
                "RestrictionRule",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Name",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'Name' in type 'RestrictionRule' is empty!",
                exception.Message
            );
        }

        private static void AssertRestoreInvalidDuration(
            string name,
            int durationMinutes)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestrictionRule.Restore(
                    name,
                    durationMinutes
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
                "RestrictionRule",
                exception.Context["Type"]
            );

            Assert.Equal(
                "DurationMinutes",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'DurationMinutes' in type 'RestrictionRule' has a value outside the allowed range!",
                exception.Message
            );
        }
        #endregion

        #region Create and Restore: valid values
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(60)]
        [InlineData(int.MaxValue - 1)]
        [InlineData(int.MaxValue)]
        public void Create_ValidArguments_PreservesProperties(int durationMinutes)
        {
            AssertCreateSuccess(
                ValidName,
                durationMinutes
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(60)]
        [InlineData(int.MaxValue - 1)]
        [InlineData(int.MaxValue)]
        public void Restore_ValidArguments_PreservesProperties(int durationMinutes)
        {
            AssertRestoreSuccess(
                ValidName,
                durationMinutes
            );
        }

        [Theory]
        [InlineData(" Rule")]
        [InlineData("Rule ")]
        [InlineData(" MiXeD Rule ")]
        [InlineData("Rule name")]
        [InlineData("Rule  name")]
        [InlineData("\tRule\r\n")]
        public void Create_WhitespaceInName_PreservesExactValue(string name)
        {
            AssertCreateSuccess(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData(" Rule")]
        [InlineData("Rule ")]
        [InlineData(" MiXeD Rule ")]
        [InlineData("Rule name")]
        [InlineData("\tRule\r\n")]
        public void Restore_WhitespaceInName_PreservesExactValue(string name)
        {
            AssertRestoreSuccess(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData("Правило 🙂")]
        [InlineData("Règle")]
        [InlineData("Re\u0300gle")]
        [InlineData("Rule №1")]
        [InlineData("123")]
        public void Create_UnicodeAndSymbolsInName_PreservesExactValue(string name)
        {
            AssertCreateSuccess(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData("Правило 🙂")]
        [InlineData("Règle")]
        [InlineData("Re\u0300gle")]
        [InlineData("Rule №1")]
        [InlineData("123")]
        public void Restore_UnicodeAndSymbolsInName_PreservesExactValue(string name)
        {
            AssertRestoreSuccess(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1000)]
        public void Create_NonEmptyNameLengths_PreservesValue(int length)
        {
            var name = new string('a', length);

            AssertCreateSuccess(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1000)]
        public void Restore_NonEmptyNameLengths_PreservesValue(int length)
        {
            var name = new string('a', length);

            AssertRestoreSuccess(
                name,
                ValidDuration
            );
        }
        #endregion

        #region Create and Restore: validation
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyName_ReportsEmptyValue(string? name)
        {
            AssertCreateEmptyName(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyName_ReportsEmptyValue(string? name)
        {
            AssertRestoreEmptyName(
                name,
                ValidDuration
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void Create_NonPositiveDuration_ReportsInvalidDuration(int durationMinutes)
        {
            AssertCreateInvalidDuration(
                ValidName,
                durationMinutes
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void Restore_NonPositiveDuration_ReportsInvalidDuration(int durationMinutes)
        {
            AssertRestoreInvalidDuration(
                ValidName,
                durationMinutes
            );
        }

        // За двох порушень порожня назва має пріоритет над некоректною тривалістю.
        // When both values are invalid, the empty name takes priority over the invalid duration.
        [Theory]
        [InlineData(null, 0)]
        [InlineData("", -1)]
        [InlineData(" ", int.MinValue)]
        [InlineData("\t\r\n", -100)]
        [InlineData("\u00A0", 0)]
        public void Create_EmptyNameAndInvalidDuration_ReportsEmptyValue(
            string? name,
            int durationMinutes)
        {
            AssertCreateEmptyName(
                name,
                durationMinutes
            );
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", -1)]
        [InlineData(" ", int.MinValue)]
        [InlineData("\t\r\n", -100)]
        [InlineData("\u00A0", 0)]
        public void Restore_EmptyNameAndInvalidDuration_ReportsEmptyValue(
            string? name,
            int durationMinutes)
        {
            AssertRestoreEmptyName(
                name,
                durationMinutes
            );
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void GetEqualityComponents_ValidRule_ReturnsComponentsInOrder()
        {
            var rule = RestrictionRule.Restore(
                ValidName,
                ValidDuration
            );

            var components = rule.GetEqualityComponents().ToArray();

            Assert.Equal(
                2,
                components.Length
            );

            Assert.Equal(
                ValidName,
                components[0]
            );

            Assert.IsType<string>(components[0]);

            Assert.Equal(
                ValidDuration,
                components[1]
            );

            Assert.IsType<int>(components[1]);
        }

        [Theory]
        [InlineData("Rule", "Other")]
        [InlineData("Rule", "rule")]
        [InlineData("Rule", " Rule ")]
        [InlineData("Rule name", "Rule  name")]
        [InlineData("Règle", "Re\u0300gle")]
        public void Equals_DifferentNames_ReturnsFalse(
            string firstName,
            string secondName)
        {
            var first = RestrictionRule.Restore(
                firstName,
                ValidDuration
            );

            var second = RestrictionRule.Restore(
                secondName,
                ValidDuration
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void Equals_DifferentDurations_ReturnsFalse()
        {
            var first = RestrictionRule.Restore(
                ValidName,
                1
            );

            var second = RestrictionRule.Restore(
                ValidName,
                2
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void CreateThenRestore_PreservesPropertiesEqualityAndHashCode()
        {
            const string name = " Rule 🙂 ";
            const int duration = 60;

            var createResult = RestrictionRule.Create(
                name,
                duration
            );

            Assert.True(createResult.IsSuccess);

            var created = createResult.Value;

            var restored = RestrictionRule.Restore(
                created.Name,
                created.DurationMinutes
            );

            Assert.Equal(
                name,
                created.Name
            );

            Assert.Equal(
                duration,
                created.DurationMinutes
            );

            Assert.Equal(
                created.Name,
                restored.Name
            );

            Assert.Equal(
                created.DurationMinutes,
                restored.DurationMinutes
            );

            Assert.NotSame(
                created,
                restored
            );

            Assert.True(created.Equals((object)restored));
            Assert.True(restored.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                restored.GetHashCode()
            );
        }
        #endregion
    }
}
