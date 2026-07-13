using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRefreshToken.ValueObjects
{
    public class IpAddressTests
    {
        #region Creation
        [Theory]
        [InlineData("192.168.1.1")]
        [InlineData("8.8.8.8")]
        [InlineData("127.0.0.1")]
        [InlineData("255.255.255.255")]
        public void Create_ShouldReturnSuccess_ForValidIPv4(string ip)
        {
            var result = IpAddress.Create(ip);

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(ip);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("256.1.2.3")]
        [InlineData("1.2.3")]
        [InlineData("1.2.3.4.5")]
        [InlineData("abc.def.ghi.jkl")]
        [InlineData("192.168.1.a")]
        public void Create_ShouldReturnFailure_ForInvalidIp(string invalidIp)
        {
            var result = IpAddress.Create(invalidIp);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "INVALID_FORMAT_IP");
        }
        #endregion

        #region Restore
        [Theory]
        [InlineData("192.168.0.1")]
        [InlineData("10.0.0.1")]
        public void Restore_ShouldReturnInstance_ForValidIp(string ip)
        {
            var ipAddress = IpAddress.Restore(ip);

            ipAddress.Value.Should().Be(ip);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Restore_ShouldThrowException_WhenValueIsEmpty(string? invalidValue)
        {
            Action act = () => IpAddress.Restore(invalidValue!);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Theory]
        [InlineData("999.1.2.3")]
        [InlineData("1.2.3")]
        public void Restore_ShouldThrowException_WhenIpFormatIsInvalid(string invalidIp)
        {
            Action act = () => IpAddress.Restore(invalidIp);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenValuesAreSame()
        {
            var ip1 = IpAddress.Create("192.168.1.100").Value;
            var ip2 = IpAddress.Create("192.168.1.100").Value;

            ip1.Should().Be(ip2);
            ip1.GetHashCode().Should().Be(ip2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenValuesAreDifferent()
        {
            var ip1 = IpAddress.Create("192.168.1.1").Value;
            var ip2 = IpAddress.Create("192.168.1.2").Value;

            ip1.Should().NotBe(ip2);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnValue()
        {
            var ip = IpAddress.Create("8.8.4.4").Value;

            var components = ip.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be("8.8.4.4");
        }
        #endregion
    }
}
