using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRefreshToken.ValueObjects
{
    public class IpAddressTests
    {
        #region Helpers
        // Спільні перевірки розрізняють Result для Create та винятки для Restore.
        // Shared assertions distinguish Create results from Restore exceptions.
        private static void AssertCreateSuccess(
            string? input,
            string expectedValue)
        {
            var result = IpAddress.Create(input);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expectedValue,
                result.Value.Value
            );
        }

        private static void AssertCreateEmptyFailure(string? input)
        {
            var result = IpAddress.Create(input);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "IP_ADDRESS.EMPTY",
                error.Code
            );

            Assert.Equal(
                "Ip address is empty or missing. Owner: IpAddress!",
                error.Message
            );
        }

        private static void AssertCreateInvalidFormat(string? input)
        {
            var result = IpAddress.Create(input);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "IP_ADDRESS.INVALID_FORMAT",
                error.Code
            );

            Assert.Equal(
                "Incorrect IP address format. Owner: IpAddress!",
                error.Message
            );
        }

        private static void AssertRestoreSuccess(string value)
        {
            var address = IpAddress.Restore(value);

            Assert.Equal(
                value,
                address.Value
            );
        }

        private static void AssertRestoreEmpty(string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                IpAddress.Restore(value!)
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
                "IpAddress",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'Value' in type 'IpAddress' is empty!",
                exception.Message
            );
        }

        private static void AssertRestoreInvalidFormat(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                IpAddress.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
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
                "IpAddress",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'Value' in type 'IpAddress' has an invalid format!",
                exception.Message
            );
        }
        #endregion

        #region Create and Restore: valid IPv4
        [Theory]
        [InlineData("0.0.0.0")]
        [InlineData("255.255.255.255")]
        [InlineData("127.0.0.1")]
        [InlineData("10.0.0.1")]
        [InlineData("192.168.1.1")]
        [InlineData("8.8.8.8")]
        [InlineData("1.10.100.255")]
        public void Create_ValidIpv4_PreservesValue(string value)
        {
            AssertCreateSuccess(
                value,
                value
            );
        }

        [Theory]
        [InlineData("0.0.0.0")]
        [InlineData("255.255.255.255")]
        [InlineData("127.0.0.1")]
        [InlineData("10.0.0.1")]
        [InlineData("192.168.1.1")]
        [InlineData("8.8.8.8")]
        [InlineData("1.10.100.255")]
        public void Restore_ValidIpv4_PreservesValue(string value)
        {
            AssertRestoreSuccess(value);
        }
        #endregion

        #region Create and Restore: required value
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? value)
        {
            AssertCreateEmptyFailure(value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? value)
        {
            AssertRestoreEmpty(value);
        }
        #endregion

        #region Create and Restore: surrounding whitespace
        // Create прибирає зовнішні пробіли, тоді як Restore вимагає вже нормалізованого значення.
        // Create trims surrounding whitespace, while Restore requires an already normalized value.
        [Theory]
        [InlineData(" 192.168.1.1")]
        [InlineData("192.168.1.1 ")]
        [InlineData("\t192.168.1.1\r\n")]
        [InlineData("\u00A0192.168.1.1\u00A0")]
        public void Create_SurroundingWhitespace_TrimsValue(string value)
        {
            AssertCreateSuccess(
                value,
                "192.168.1.1"
            );
        }

        [Theory]
        [InlineData(" 192.168.1.1")]
        [InlineData("192.168.1.1 ")]
        [InlineData("\t192.168.1.1\r\n")]
        [InlineData("\u00A0192.168.1.1\u00A0")]
        public void Restore_SurroundingWhitespace_ThrowsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }
        #endregion

        #region Create and Restore: IPv4 octets
        [Theory]
        [InlineData("127")]
        [InlineData("127.1")]
        [InlineData("127.0.1")]
        [InlineData("127.0.0.1.2")]
        [InlineData("2130706433")]
        public void Create_InvalidOctetCount_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("127")]
        [InlineData("127.1")]
        [InlineData("127.0.1")]
        [InlineData("127.0.0.1.2")]
        [InlineData("2130706433")]
        public void Restore_InvalidOctetCount_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData(".2.3.4")]
        [InlineData("1..3.4")]
        [InlineData("1.2..4")]
        [InlineData("1.2.3.")]
        public void Create_EmptyOctet_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData(".2.3.4")]
        [InlineData("1..3.4")]
        [InlineData("1.2..4")]
        [InlineData("1.2.3.")]
        public void Restore_EmptyOctet_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData("1000.2.3.4")]
        [InlineData("1.1000.3.4")]
        [InlineData("1.2.1000.4")]
        [InlineData("1.2.3.1000")]
        public void Create_TooLongOctet_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("1000.2.3.4")]
        [InlineData("1.1000.3.4")]
        [InlineData("1.2.1000.4")]
        [InlineData("1.2.3.1000")]
        public void Restore_TooLongOctet_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData("256.2.3.4")]
        [InlineData("1.256.3.4")]
        [InlineData("1.2.256.4")]
        [InlineData("1.2.3.256")]
        [InlineData("999.2.3.4")]
        [InlineData("1.999.3.4")]
        [InlineData("1.2.999.4")]
        [InlineData("1.2.3.999")]
        public void Create_OctetAbove255_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("256.2.3.4")]
        [InlineData("1.256.3.4")]
        [InlineData("1.2.256.4")]
        [InlineData("1.2.3.256")]
        [InlineData("999.2.3.4")]
        [InlineData("1.999.3.4")]
        [InlineData("1.2.999.4")]
        [InlineData("1.2.3.999")]
        public void Restore_OctetAbove255_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData("00.2.3.4")]
        [InlineData("1.00.3.4")]
        [InlineData("1.2.00.4")]
        [InlineData("1.2.3.00")]
        [InlineData("01.2.3.4")]
        [InlineData("1.01.3.4")]
        [InlineData("1.2.01.4")]
        [InlineData("1.2.3.01")]
        [InlineData("001.2.3.4")]
        [InlineData("1.001.3.4")]
        [InlineData("1.2.001.4")]
        [InlineData("1.2.3.001")]
        public void Create_LeadingZero_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("00.2.3.4")]
        [InlineData("1.00.3.4")]
        [InlineData("1.2.00.4")]
        [InlineData("1.2.3.00")]
        [InlineData("01.2.3.4")]
        [InlineData("1.01.3.4")]
        [InlineData("1.2.01.4")]
        [InlineData("1.2.3.01")]
        [InlineData("001.2.3.4")]
        [InlineData("1.001.3.4")]
        [InlineData("1.2.001.4")]
        [InlineData("1.2.3.001")]
        public void Restore_LeadingZero_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }
        #endregion

        #region Create and Restore: characters and whitespace
        [Theory]
        [InlineData("١.2.3.4")]
        [InlineData("۱.2.3.4")]
        [InlineData("１.2.3.4")]
        public void Create_NonAsciiDigits_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("١.2.3.4")]
        [InlineData("۱.2.3.4")]
        [InlineData("１.2.3.4")]
        public void Restore_NonAsciiDigits_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData("abc.2.3.4")]
        [InlineData("+1.2.3.4")]
        [InlineData("-1.2.3.4")]
        [InlineData("1e2.2.3.4")]
        public void Create_NonDigitCharacters_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("abc.2.3.4")]
        [InlineData("+1.2.3.4")]
        [InlineData("-1.2.3.4")]
        [InlineData("1e2.2.3.4")]
        public void Restore_NonDigitCharacters_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData("1. 2.3.4")]
        [InlineData("1.2 .3.4")]
        [InlineData("1.2.\t3.4")]
        [InlineData("1.2.3.\n4")]
        public void Create_InternalWhitespace_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("1. 2.3.4")]
        [InlineData("1.2 .3.4")]
        [InlineData("1.2.\t3.4")]
        [InlineData("1.2.3.\n4")]
        public void Restore_InternalWhitespace_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }
        #endregion

        #region Create and Restore: unsupported address forms
        [Theory]
        [InlineData("::1")]
        [InlineData("2001:db8::1")]
        [InlineData("::ffff:192.168.1.1")]
        public void Create_Ipv6_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("::1")]
        [InlineData("2001:db8::1")]
        [InlineData("::ffff:192.168.1.1")]
        public void Restore_Ipv6_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }

        [Theory]
        [InlineData("127.0.0.1:80")]
        [InlineData("192.168.1.1/24")]
        public void Create_PortOrCidr_ReportsInvalidFormat(string value)
        {
            AssertCreateInvalidFormat(value);
        }

        [Theory]
        [InlineData("127.0.0.1:80")]
        [InlineData("192.168.1.1/24")]
        public void Restore_PortOrCidr_ReportsInvalidFormat(string value)
        {
            AssertRestoreInvalidFormat(value);
        }
        #endregion

        #region Equality
        [Fact]
        public void GetEqualityComponents_ValidAddress_ReturnsSingleStringComponent()
        {
            var address = IpAddress.Restore("192.168.1.1");

            var component = Assert.Single(address.GetEqualityComponents());

            Assert.Equal(
                "192.168.1.1",
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void CreateThenRestore_PreservesValueEqualityAndHashCode()
        {
            var createResult = IpAddress.Create("\t192.168.1.1\r\n");

            Assert.True(createResult.IsSuccess);

            var created = createResult.Value;

            var restored = IpAddress.Restore(created.Value);

            Assert.Equal(
                "192.168.1.1",
                created.Value
            );

            Assert.Equal(
                "192.168.1.1",
                restored.Value
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

        [Theory]
        [InlineData("192.168.1.1", "192.168.1.2")]
        [InlineData("0.0.0.0", "255.255.255.255")]
        public void Equals_DifferentAddresses_ReturnsFalse(
            string firstValue,
            string secondValue)
        {
            var first = IpAddress.Restore(firstValue);
            var second = IpAddress.Restore(secondValue);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion
    }
}
