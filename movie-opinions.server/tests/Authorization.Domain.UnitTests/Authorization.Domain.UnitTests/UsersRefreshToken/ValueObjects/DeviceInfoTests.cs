using Authorization.Domain.UsersRefreshToken.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRefreshToken.ValueObjects
{
    public class DeviceInfoTests
    {
        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WithProvidedData()
        {
            var result = DeviceInfo.Create("Mobile", "Android 13", "Chrome", "Pixel 7");

            result.IsSuccess.Should().BeTrue();
            var device = result.Value;

            device.DeviceType.Should().Be("Mobile");
            device.OperatingSystem.Should().Be("Android 13");
            device.Browser.Should().Be("Chrome");
            device.DeviceModel.Should().Be("Pixel 7");
        }

        [Fact]
        public void Create_ShouldAccept_EmptyOrNullStrings()
        {
            var result = DeviceInfo.Create("", "", "", "");

            result.IsSuccess.Should().BeTrue();
            var device = result.Value;

            device.DeviceType.Should().Be("");
            device.OperatingSystem.Should().Be("");
            device.Browser.Should().Be("");
            device.DeviceModel.Should().Be("");
        }
        #endregion

        #region ToDisplayString
        [Fact]
        public void ToDisplayString_ShouldReturnCorrectFormat_WithModel()
        {
            var device = DeviceInfo.Create("Desktop", "Windows 11", "Firefox", "Custom PC").Value;

            var display = device.ToDisplayString();

            display.Should().Be("Custom PC, Windows 11 (Firefox)");
        }

        [Fact]
        public void ToDisplayString_ShouldReturnCorrectFormat_WithoutModel()
        {
            var device = DeviceInfo.Create("Mobile", "iOS 17", "Safari", "").Value;

            var display = device.ToDisplayString();

            display.Should().Be("iOS 17 (Safari)");
        }

        [Fact]
        public void ToDisplayString_ShouldHandle_EmptyValues()
        {
            var device = DeviceInfo.Create("", "", "", "").Value;

            var display = device.ToDisplayString();

            display.Should().Be(" ()");
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenAllFieldsAreSame()
        {
            var device1 = DeviceInfo.Create("Mobile", "Android", "Chrome", "Pixel").Value;
            var device2 = DeviceInfo.Create("Mobile", "Android", "Chrome", "Pixel").Value;

            device1.Should().Be(device2);
            device1.GetHashCode().Should().Be(device2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenAnyFieldDiffers()
        {
            var device1 = DeviceInfo.Create("Mobile", "Android", "Chrome", "Pixel").Value;
            var device2 = DeviceInfo.Create("Mobile", "Android", "Chrome", "Samsung").Value;

            device1.Should().NotBe(device2);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnAllProperties()
        {
            var device = DeviceInfo.Create("Tablet", "iPadOS", "Safari", "iPad Air").Value;

            var components = device.GetEqualityComponents().ToList();

            components.Should().HaveCount(4);
            components.Should().Contain("Tablet");
            components.Should().Contain("iPadOS");
            components.Should().Contain("Safari");
            components.Should().Contain("iPad Air");
        }
        #endregion
    }
}
