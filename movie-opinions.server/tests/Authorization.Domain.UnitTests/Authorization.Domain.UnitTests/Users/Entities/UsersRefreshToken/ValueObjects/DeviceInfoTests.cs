using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRefreshToken.ValueObjects
{
    public class DeviceInfoTests
    {
        #region Test data
        private const string ValidOs = "Windows 11";
        private const string ValidBrowser = "Chrome";
        private const string ValidModel = "Model X";
        #endregion

        #region Helpers
        // Перевірки враховують різні контракти помилок Create і Restore.
        // Assertions account for the different Create and Restore failure contracts.
        private static void AssertCreateSuccess(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var result = DeviceInfo.Create(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                deviceType,
                result.Value.DeviceType
            );

            Assert.Equal(
                operatingSystem,
                result.Value.OperatingSystem
            );

            Assert.Equal(
                browser,
                result.Value.Browser
            );

            Assert.Equal(
                deviceModel,
                result.Value.DeviceModel
            );
        }

        private static void AssertRestoreSuccess(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var info = DeviceInfo.Restore(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            Assert.Equal(
                deviceType,
                info.DeviceType
            );

            Assert.Equal(
                operatingSystem,
                info.OperatingSystem
            );

            Assert.Equal(
                browser,
                info.Browser
            );

            Assert.Equal(
                deviceModel,
                info.DeviceModel
            );
        }

        private static void AssertCreateValidationFailure(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel,
            string expectedCode)
        {
            var result = DeviceInfo.Create(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                expectedCode,
                error.Code
            );

            const string prefix =
                "Device information validation failed for type 'DeviceInfo': ";

            var expectedMessage = expectedCode switch
            {
                "DEVICE_INFO.EMPTY_OPERATING_SYSTEM" =>
                    prefix + "operating system is required.",

                "DEVICE_INFO.EMPTY_BROWSER" =>
                    prefix + "browser is required.",

                "DEVICE_INFO.EMPTY_DEVICE_MODEL" =>
                    prefix + "device model is required.",

                "DEVICE_INFO.INVALID_DEVICE_TYPE" =>
                    prefix + "device type is invalid or unsupported.",

                "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM" =>
                    prefix + $"operating system length is {operatingSystem.Length}, " +
                    "but the maximum allowed is 100.",

                "DEVICE_INFO.TOO_LONG_BROWSER" =>
                    prefix + $"browser length is {browser.Length}, " +
                    "but the maximum allowed is 100.",

                "DEVICE_INFO.TOO_LONG_DEVICE_MODEL" =>
                    prefix + $"device model length is {deviceModel.Length}, " +
                    "but the maximum allowed is 200.",

                _ => throw new InvalidOperationException(
                    "Unknown validation code in test data."
                )
            };

            Assert.Equal(expectedMessage, error.Message);
        }

        private static void AssertRestoreEmptyField(
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
                "DeviceInfo",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                $"The field '{field}' in type 'DeviceInfo' is empty!",
                exception.Message
            );
        }

        private static void AssertRestoreUnsupportedType(
            DomainDataInconsistencyException exception,
            DeviceType expectedValue)
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
                "DeviceInfo",
                exception.Context["Type"]
            );

            Assert.Equal(
                "DeviceType",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                expectedValue,
                exception.Context["Value"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                $"Unsupported discriminator value '{expectedValue}' for type 'DeviceInfo'!",
                exception.Message
            );
        }

        private static void AssertRestoreOutOfRange(
            DomainDataInconsistencyException exception,
            string field,
            int actualLength,
            int maximumLength)
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
                "DeviceInfo",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
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
                actualLength,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                maximumLength,
                exception.Context["Custom_MaximumLength"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                $"The field '{field}' in type 'DeviceInfo' has a value outside the allowed range!",
                exception.Message
            );
        }
        #endregion

        #region Create and Restore: valid arguments
        [Theory]
        [InlineData(DeviceType.Desktop)]
        [InlineData(DeviceType.Mobile)]
        [InlineData(DeviceType.Tablet)]
        [InlineData(DeviceType.Unknown)]
        public void Create_ValidArguments_PreservesAllProperties(DeviceType deviceType)
        {
            AssertCreateSuccess(deviceType, ValidOs, ValidBrowser, ValidModel);
        }

        [Theory]
        [InlineData(DeviceType.Desktop)]
        [InlineData(DeviceType.Mobile)]
        [InlineData(DeviceType.Tablet)]
        [InlineData(DeviceType.Unknown)]
        public void Restore_ValidArguments_PreservesAllProperties(DeviceType deviceType)
        {
            AssertRestoreSuccess(deviceType, ValidOs, ValidBrowser, ValidModel);
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(99, 99, 199)]
        [InlineData(100, 100, 200)]
        public void Create_BoundaryLengths_Succeeds(
            int osLength,
            int browserLength,
            int modelLength)
        {
            AssertCreateSuccess(
                DeviceType.Desktop,
                new string('a', osLength),
                new string('b', browserLength),
                new string('c', modelLength)
            );
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(99, 99, 199)]
        [InlineData(100, 100, 200)]
        public void Restore_BoundaryLengths_Succeeds(
            int osLength,
            int browserLength,
            int modelLength)
        {
            AssertRestoreSuccess(
                DeviceType.Desktop,
                new string('a', osLength),
                new string('b', browserLength),
                new string('c', modelLength)
            );
        }
        #endregion

        #region Create and Restore: exact stored values
        [Fact]
        public void Create_WhitespaceAndCase_PreservesExactValues()
        {
            AssertCreateSuccess(
                DeviceType.Mobile,
                " Windows 11 ",
                "\tChrome\r\n",
                "Model  X"
            );
        }

        [Fact]
        public void Restore_WhitespaceAndCase_PreservesExactValues()
        {
            AssertRestoreSuccess(
                DeviceType.Mobile,
                " Windows 11 ",
                "\tChrome\r\n",
                "Model  X"
            );
        }

        [Fact]
        public void Create_UnicodeAndSymbols_PreservesExactValues()
        {
            AssertCreateSuccess(
                DeviceType.Tablet,
                "Windows 11 Pro™",
                "Браузер Chrome",
                "iPhone 15 Pro 🙂"
            );
        }

        [Fact]
        public void Restore_UnicodeAndSymbols_PreservesExactValues()
        {
            AssertRestoreSuccess(
                DeviceType.Tablet,
                "Windows 11 Pro™",
                "Браузер Chrome",
                "iPhone 15 Pro 🙂"
            );
        }
        #endregion

        #region Create and Restore: length semantics
        [Theory]
        [InlineData("OperatingSystem", 100)]
        [InlineData("Browser", 100)]
        [InlineData("DeviceModel", 200)]
        public void Create_WhitespaceCountsTowardLength(
            string field,
            int maxLength)
        {
            var valid = "A" + new string(' ', maxLength - 1);
            var tooLong = "A" + new string(' ', maxLength);

            var validOs = field == "OperatingSystem" ? valid : ValidOs;
            var validBrowser = field == "Browser" ? valid : ValidBrowser;
            var validModel = field == "DeviceModel" ? valid : ValidModel;

            AssertCreateSuccess(
                DeviceType.Desktop,
                validOs,
                validBrowser,
                validModel
            );

            var longOs = field == "OperatingSystem" ? tooLong : ValidOs;
            var longBrowser = field == "Browser" ? tooLong : ValidBrowser;
            var longModel = field == "DeviceModel" ? tooLong : ValidModel;

            var expectedCode = field switch
            {
                "OperatingSystem" => "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM",
                "Browser" => "DEVICE_INFO.TOO_LONG_BROWSER",
                _ => "DEVICE_INFO.TOO_LONG_DEVICE_MODEL"
            };

            AssertCreateValidationFailure(
                DeviceType.Desktop,
                longOs,
                longBrowser,
                longModel,
                expectedCode
            );
        }

        [Theory]
        [InlineData("OperatingSystem", 100)]
        [InlineData("Browser", 100)]
        [InlineData("DeviceModel", 200)]
        public void Restore_WhitespaceCountsTowardLength(
            string field,
            int maxLength)
        {
            var valid = "A" + new string(' ', maxLength - 1);
            var tooLong = "A" + new string(' ', maxLength);

            var validOs = field == "OperatingSystem" ? valid : ValidOs;
            var validBrowser = field == "Browser" ? valid : ValidBrowser;
            var validModel = field == "DeviceModel" ? valid : ValidModel;

            AssertRestoreSuccess(
                DeviceType.Desktop,
                validOs,
                validBrowser,
                validModel
            );

            var longOs = field == "OperatingSystem" ? tooLong : ValidOs;
            var longBrowser = field == "Browser" ? tooLong : ValidBrowser;
            var longModel = field == "DeviceModel" ? tooLong : ValidModel;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    longOs,
                    longBrowser,
                    longModel
                )
            );

            AssertRestoreOutOfRange(
                exception,
                field,
                maxLength + 1,
                maxLength
            );
        }

        // Один emoji займає дві кодові одиниці UTF-16 у String.Length.
        // One emoji occupies two UTF-16 code units in String.Length.
        [Theory]
        [InlineData("OperatingSystem", 100)]
        [InlineData("Browser", 100)]
        [InlineData("DeviceModel", 200)]
        public void Create_SupplementaryCharacters_LengthUsesUtf16CodeUnits(
            string field,
            int maxLength)
        {
            var valid = new string('a', maxLength - 2) + "🙂";
            var invalid = new string('a', maxLength - 1) + "🙂";

            Assert.Equal(
                maxLength,
                valid.Length
            );

            Assert.Equal(
                maxLength + 1,
                invalid.Length
            );

            var validOs = field == "OperatingSystem" ? valid : ValidOs;
            var validBrowser = field == "Browser" ? valid : ValidBrowser;
            var validModel = field == "DeviceModel" ? valid : ValidModel;

            AssertCreateSuccess(
                DeviceType.Desktop,
                validOs,
                validBrowser,
                validModel
            );

            var invalidOs = field == "OperatingSystem" ? invalid : ValidOs;
            var invalidBrowser = field == "Browser" ? invalid : ValidBrowser;
            var invalidModel = field == "DeviceModel" ? invalid : ValidModel;

            var expectedCode = field switch
            {
                "OperatingSystem" => "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM",
                "Browser" => "DEVICE_INFO.TOO_LONG_BROWSER",
                _ => "DEVICE_INFO.TOO_LONG_DEVICE_MODEL"
            };

            AssertCreateValidationFailure(
                DeviceType.Desktop,
                invalidOs,
                invalidBrowser,
                invalidModel,
                expectedCode
            );
        }

        // Для збережених рядків межа також рахується в кодових одиницях UTF-16.
        // Stored string boundaries are also measured in UTF-16 code units.
        [Theory]
        [InlineData("OperatingSystem", 100)]
        [InlineData("Browser", 100)]
        [InlineData("DeviceModel", 200)]
        public void Restore_SupplementaryCharacters_LengthUsesUtf16CodeUnits(
            string field,
            int maxLength)
        {
            var valid = new string('a', maxLength - 2) + "🙂";
            var invalid = new string('a', maxLength - 1) + "🙂";

            var validOs = field == "OperatingSystem" ? valid : ValidOs;
            var validBrowser = field == "Browser" ? valid : ValidBrowser;
            var validModel = field == "DeviceModel" ? valid : ValidModel;

            AssertRestoreSuccess(
                DeviceType.Desktop,
                validOs,
                validBrowser,
                validModel
            );

            var invalidOs = field == "OperatingSystem" ? invalid : ValidOs;
            var invalidBrowser = field == "Browser" ? invalid : ValidBrowser;
            var invalidModel = field == "DeviceModel" ? invalid : ValidModel;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    invalidOs,
                    invalidBrowser,
                    invalidModel
                )
            );

            AssertRestoreOutOfRange(
                exception,
                field,
                maxLength + 1,
                maxLength
            );
        }
        #endregion

        #region Create and Restore: required fields
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyOperatingSystem_ReportsEmptyValue(string? value)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                value!,
                ValidBrowser,
                ValidModel,
                "DEVICE_INFO.EMPTY_OPERATING_SYSTEM"
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyOperatingSystem_ReportsEmptyValue(string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    value!,
                    ValidBrowser,
                    ValidModel
                )
            );

            AssertRestoreEmptyField(
                exception,
                "OperatingSystem"
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyBrowser_ReportsEmptyValue(string? value)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                ValidOs,
                value!,
                ValidModel,
                "DEVICE_INFO.EMPTY_BROWSER"
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyBrowser_ReportsEmptyValue(string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    ValidOs,
                    value!,
                    ValidModel
                )
            );

            AssertRestoreEmptyField(
                exception,
                "Browser"
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyDeviceModel_ReportsEmptyValue(string? value)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                ValidOs,
                ValidBrowser,
                value!,
                "DEVICE_INFO.EMPTY_DEVICE_MODEL"
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyDeviceModel_ReportsEmptyValue(string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    ValidOs,
                    ValidBrowser,
                    value!
                )
            );

            AssertRestoreEmptyField(
                exception,
                "DeviceModel"
            );
        }
        #endregion

        #region Create and Restore: device type
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Create_UndefinedDeviceType_ReportsUnsupportedType(int typeValue)
        {
            AssertCreateValidationFailure(
                (DeviceType)typeValue,
                ValidOs,
                ValidBrowser,
                ValidModel,
                "DEVICE_INFO.INVALID_DEVICE_TYPE"
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Restore_UndefinedDeviceType_ReportsUnsupportedType(int typeValue)
        {
            var deviceType = (DeviceType)typeValue;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    deviceType,
                    ValidOs,
                    ValidBrowser,
                    ValidModel
                )
            );

            AssertRestoreUnsupportedType(
                exception,
                deviceType
            );
        }
        #endregion

        #region Create and Restore: maximum lengths
        [Theory]
        [InlineData(101)]
        [InlineData(102)]
        [InlineData(150)]
        public void Create_TooLongOperatingSystem_ReportsLengthFailure(int length)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                new string('a', length),
                ValidBrowser,
                ValidModel,
                "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM"
            );
        }

        [Theory]
        [InlineData(101)]
        [InlineData(102)]
        [InlineData(150)]
        public void Restore_TooLongOperatingSystem_ReportsLengthFailure(int length)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    new string('a', length),
                    ValidBrowser,
                    ValidModel
                )
            );

            AssertRestoreOutOfRange(
                exception,
                "OperatingSystem",
                length,
                100
            );
        }

        [Theory]
        [InlineData(101)]
        [InlineData(102)]
        [InlineData(150)]
        public void Create_TooLongBrowser_ReportsLengthFailure(int length)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                ValidOs,
                new string('b', length),
                ValidModel,
                "DEVICE_INFO.TOO_LONG_BROWSER"
            );
        }

        [Theory]
        [InlineData(101)]
        [InlineData(102)]
        [InlineData(150)]
        public void Restore_TooLongBrowser_ReportsLengthFailure(int length)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    ValidOs,
                    new string('b', length),
                    ValidModel
                )
            );

            AssertRestoreOutOfRange(
                exception,
                "Browser",
                length,
                100
            );
        }

        [Theory]
        [InlineData(201)]
        [InlineData(202)]
        [InlineData(250)]
        public void Create_TooLongDeviceModel_ReportsLengthFailure(int length)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                ValidOs,
                ValidBrowser,
                new string('c', length),
                "DEVICE_INFO.TOO_LONG_DEVICE_MODEL"
            );
        }

        [Theory]
        [InlineData(201)]
        [InlineData(202)]
        [InlineData(250)]
        public void Restore_TooLongDeviceModel_ReportsLengthFailure(int length)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    ValidOs,
                    ValidBrowser,
                    new string('c', length)
                )
            );

            AssertRestoreOutOfRange(
                exception,
                "DeviceModel",
                length,
                200
            );
        }
        #endregion

        #region Validation order
        // Кілька порушень задаються разом, щоб перевірити пріоритет повідомленої помилки.
        // Multiple violations are combined to verify which error takes precedence.
        [Theory]
        [InlineData(true, true, false, "DEVICE_INFO.EMPTY_OPERATING_SYSTEM")]
        [InlineData(true, false, true, "DEVICE_INFO.EMPTY_OPERATING_SYSTEM")]
        [InlineData(false, true, true, "DEVICE_INFO.EMPTY_BROWSER")]
        [InlineData(true, true, true, "DEVICE_INFO.EMPTY_OPERATING_SYSTEM")]
        public void Create_MultipleEmptyFields_ReportsFirstEmptyField(
            bool emptyOs,
            bool emptyBrowser,
            bool emptyModel,
            string expectedCode)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                emptyOs ? " " : ValidOs,
                emptyBrowser ? " " : ValidBrowser,
                emptyModel ? " " : ValidModel,
                expectedCode
            );
        }

        [Theory]
        [InlineData(true, true, false, "OperatingSystem")]
        [InlineData(true, false, true, "OperatingSystem")]
        [InlineData(false, true, true, "Browser")]
        [InlineData(true, true, true, "OperatingSystem")]
        public void Restore_MultipleEmptyFields_ReportsFirstEmptyField(
            bool emptyOs,
            bool emptyBrowser,
            bool emptyModel,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    emptyOs ? " " : ValidOs,
                    emptyBrowser ? " " : ValidBrowser,
                    emptyModel ? " " : ValidModel
                )
            );

            AssertRestoreEmptyField(
                exception,
                expectedField
            );
        }

        [Theory]
        [InlineData("OperatingSystem", "DEVICE_INFO.EMPTY_OPERATING_SYSTEM")]
        [InlineData("Browser", "DEVICE_INFO.EMPTY_BROWSER")]
        [InlineData("DeviceModel", "DEVICE_INFO.EMPTY_DEVICE_MODEL")]
        public void Create_EmptyFieldAndOtherViolations_ReportsPresenceFailure(
            string emptyField,
            string expectedCode)
        {
            AssertCreateValidationFailure(
                (DeviceType)999,
                emptyField == "OperatingSystem" ? " " : new string('a', 150),
                emptyField == "Browser" ? " " : new string('b', 150),
                emptyField == "DeviceModel" ? " " : new string('c', 250),
                expectedCode
            );
        }

        [Theory]
        [InlineData("OperatingSystem")]
        [InlineData("Browser")]
        [InlineData("DeviceModel")]
        public void Restore_EmptyFieldAndOtherViolations_ReportsPresenceFailure(string emptyField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    (DeviceType)999,
                    emptyField == "OperatingSystem" ? " " : new string('a', 150),
                    emptyField == "Browser" ? " " : new string('b', 150),
                    emptyField == "DeviceModel" ? " " : new string('c', 250)
                )
            );

            AssertRestoreEmptyField(
                exception,
                emptyField
            );
        }

        [Fact]
        public void Create_UndefinedTypeAndTooLongFields_ReportsUnsupportedType()
        {
            AssertCreateValidationFailure(
                (DeviceType)(-1),
                new string('a', 150),
                new string('b', 150),
                new string('c', 250),
                "DEVICE_INFO.INVALID_DEVICE_TYPE"
            );
        }

        [Fact]
        public void Restore_UndefinedTypeAndTooLongFields_ReportsUnsupportedType()
        {
            var deviceType = (DeviceType)(-1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    deviceType,
                    new string('a', 150),
                    new string('b', 150),
                    new string('c', 250)
                )
            );

            AssertRestoreUnsupportedType(
                exception,
                deviceType
            );
        }

        [Theory]
        [InlineData(true, true, false, "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM")]
        [InlineData(true, false, true, "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM")]
        [InlineData(false, true, true, "DEVICE_INFO.TOO_LONG_BROWSER")]
        [InlineData(true, true, true, "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM")]
        public void Create_MultipleTooLongFields_ReportsFirstLengthFailure(
            bool longOs,
            bool longBrowser,
            bool longModel,
            string expectedCode)
        {
            AssertCreateValidationFailure(
                DeviceType.Desktop,
                longOs ? new string('a', 150) : ValidOs,
                longBrowser ? new string('b', 150) : ValidBrowser,
                longModel ? new string('c', 250) : ValidModel,
                expectedCode
            );
        }

        [Theory]
        [InlineData(true, true, false, "OperatingSystem", 150, 100)]
        [InlineData(true, false, true, "OperatingSystem", 150, 100)]
        [InlineData(false, true, true, "Browser", 150, 100)]
        [InlineData(true, true, true, "OperatingSystem", 150, 100)]
        public void Restore_MultipleTooLongFields_ReportsFirstLengthFailure(
            bool longOs,
            bool longBrowser,
            bool longModel,
            string expectedField,
            int actualLength,
            int maximumLength)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeviceInfo.Restore(
                    DeviceType.Desktop,
                    longOs ? new string('a', 150) : ValidOs,
                    longBrowser ? new string('b', 150) : ValidBrowser,
                    longModel ? new string('c', 250) : ValidModel
                )
            );

            AssertRestoreOutOfRange(
                exception,
                expectedField,
                actualLength,
                maximumLength
            );
        }
        #endregion

        #region Equality
        [Fact]
        public void GetEqualityComponents_ValidDeviceInfo_ReturnsComponentsInOrder()
        {
            var result = DeviceInfo.Create(
                DeviceType.Mobile,
                ValidOs,
                ValidBrowser,
                ValidModel
            );
            Assert.True(result.IsSuccess);

            var components = result.Value.GetEqualityComponents().ToArray();

            Assert.Equal(
                4,
                components.Length
            );

            Assert.Equal(
                DeviceType.Mobile,
                components[0]
            );

            Assert.IsType<DeviceType>(components[0]);

            Assert.Equal(
                ValidOs,
                components[1]
            );
            Assert.IsType<string>(components[1]);

            Assert.Equal(
                ValidBrowser,
                components[2]
            );

            Assert.IsType<string>(components[2]);

            Assert.Equal(
                ValidModel,
                components[3]
            );

            Assert.IsType<string>(components[3]);
        }

        [Fact]
        public void CreateThenRestore_PreservesValuesEqualityAndHashCode()
        {
            var createResult = DeviceInfo.Create(
                DeviceType.Tablet,
                ValidOs,
                ValidBrowser,
                ValidModel
            );

            Assert.True(createResult.IsSuccess);

            var created = createResult.Value;

            var restored = DeviceInfo.Restore(
                created.DeviceType,
                created.OperatingSystem,
                created.Browser,
                created.DeviceModel
            );

            Assert.Equal(
                created.DeviceType,
                restored.DeviceType
            );

            Assert.Equal(
                created.OperatingSystem,
                restored.OperatingSystem
            );

            Assert.Equal(
                created.Browser,
                restored.Browser
            );

            Assert.Equal(
                created.DeviceModel,
                restored.DeviceModel
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
        [InlineData("type")]
        [InlineData("os")]
        [InlineData("os-case")]
        [InlineData("os-space")]
        [InlineData("browser")]
        [InlineData("browser-case")]
        [InlineData("model")]
        [InlineData("model-space")]
        public void Equals_SingleDifferentComponent_ReturnsFalse(string difference)
        {
            var first = DeviceInfo.Restore(
                DeviceType.Desktop,
                ValidOs,
                ValidBrowser,
                ValidModel
            );

            var second = difference switch
            {
                "type" => DeviceInfo.Restore(
                    DeviceType.Mobile, ValidOs, ValidBrowser, ValidModel),

                "os" => DeviceInfo.Restore(
                    DeviceType.Desktop, "Linux", ValidBrowser, ValidModel),

                "os-case" => DeviceInfo.Restore(
                    DeviceType.Desktop, "windows 11", ValidBrowser, ValidModel),

                "os-space" => DeviceInfo.Restore(
                    DeviceType.Desktop, "Windows  11", ValidBrowser, ValidModel),

                "browser" => DeviceInfo.Restore(
                    DeviceType.Desktop, ValidOs, "Firefox", ValidModel),

                "browser-case" => DeviceInfo.Restore(
                    DeviceType.Desktop, ValidOs, "chrome", ValidModel),

                "model" => DeviceInfo.Restore(
                    DeviceType.Desktop, ValidOs, ValidBrowser, "Model Y"),

                _ => DeviceInfo.Restore(
                    DeviceType.Desktop, ValidOs, ValidBrowser, "Model  X")
            };

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion
    }
}
