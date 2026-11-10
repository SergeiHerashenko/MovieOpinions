using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.LoginUser
{
    public class PhoneLoginTests
    {
        #region Restore: value and component validation
        [Theory]
        [InlineData("+380", "981234567", "+380981234567")]
        [InlineData("+1", "2025550123", "+12025550123")]
        [InlineData("+7", "7012345678", "+77012345678")]
        public void Restore_ValidPhone_SetsProperties(
            string code,
            string number,
            string expectedValue)
        {
            var login = PhoneLogin.Restore(
                code,
                number
            );

            Assert.Equal(
                code,
                login.Phone.PhoneCountryCode.Value
            );

            Assert.Equal(
                number,
                login.Phone.PhoneNationalNumber.Value
            );

            Assert.Equal(
                expectedValue,
                login.Value
            );

            Assert.Equal(
                LoginType.Phone,
                login.Type
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyCountryCode_PropagatesDomainDataInconsistencyException(string? code)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    code!,
                    "981234567"
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
                "PhoneCountryCode",
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
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyNationalNumber_PropagatesDomainDataInconsistencyException(string? number)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+380",
                    number!
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
                "PhoneNationalNumber",
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
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public void Restore_BothPartsEmpty_ReportsCountryCode(
            string? code,
            string? number)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    code!,
                    number!
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneCountryCode",
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
        }

        [Theory]
        [InlineData("380")]
        [InlineData("+0")]
        [InlineData("++1")]
        [InlineData(" +380")]
        [InlineData("+380 ")]
        [InlineData("＋380")]
        public void Restore_InvalidCountryCodeFormat_PropagatesDomainDataInconsistencyException(string code)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    code,
                    "981234567"
                )
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
                "PhoneCountryCode",
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
        }

        [Fact]
        public void Restore_TooShortCountryCode_PropagatesDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+",
                    "981234567"
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneCountryCode",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                1,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                2,
                exception.Context["Custom_MinimumLength"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_TooLongCountryCode_PropagatesDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+1234",
                    "981234567"
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneCountryCode",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                5,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                4,
                exception.Context["Custom_MaximumLength"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("12a34")]
        [InlineData("+1234")]
        [InlineData("١٢٣٤")]
        public void Restore_InvalidNationalNumberFormat_PropagatesDomainDataInconsistencyException(string number)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+380",
                    number
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneNationalNumber",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("(98) 123-45-67")]
        [InlineData("98-123-45-67")]
        [InlineData(" 981234567 ")]
        public void Restore_FormattedNationalNumber_PropagatesDomainDataInconsistencyException(string number)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+380",
                    number
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneNationalNumber",
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
        }

        [Fact]
        public void Restore_TooFewNationalDigits_PropagatesDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+380",
                    "123"
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneNationalNumber",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                3,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                4,
                exception.Context["Custom_MinimumDigitCount"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_TooManyNationalDigits_PropagatesDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+380",
                    new string('1', 14)
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneNationalNumber",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                14,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                13,
                exception.Context["Custom_MaximumDigitCount"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("+999", "123456789", "UnknownCountryCode")]
        [InlineData("+380", "12345678", "NationalNumberTooShort")]
        [InlineData("+380", "1234567890", "NationalNumberTooLong")]
        [InlineData("+231", "12345678", "UnsupportedNationalNumberLength")]
        public void Restore_InvalidNumberingPlan_PropagatesDomainDataInconsistencyException(
            string code,
            string number,
            string expectedReason)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    code,
                    number
                )
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
                "Phone",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Phone",
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
                code,
                exception.Context["Custom_CountryCode"]
            );

            Assert.Equal(
                number.Length,
                exception.Context["Custom_NationalNumberLength"]
            );

            Assert.Equal(
                expectedReason,
                exception.Context["Custom_NumberingPlanStatus"]
            );

            Assert.Equal(
                "NumberingPlanPhoneRule",
                exception.Context["Custom_RuleName"]
            );

            Assert.Contains(
                expectedReason,
                exception.Message
            );
        }
        #endregion

        #region Restore: stored business state and validation priority
        // Відновлення логіна зберігає історичний номер без бізнес-перевірок Create.
        // Login restoration preserves a historical phone without the Create business checks.
        [Theory]
        [InlineData("+7", "9012345678", "+79012345678")]
        [InlineData("+7", "3012345678", "+73012345678")]
        public void Restore_BlockedPhone_PreservesPhoneLogin(
            string code,
            string number,
            string expectedValue)
        {
            var login = PhoneLogin.Restore(
                code,
                number
            );

            Assert.Equal(
                code,
                login.Phone.PhoneCountryCode.Value
            );

            Assert.Equal(
                number,
                login.Phone.PhoneNationalNumber.Value
            );

            Assert.Equal(
                expectedValue,
                login.Value
            );

            Assert.Equal(
                LoginType.Phone,
                login.Type
            );
        }

        [Fact]
        public void Restore_InvalidCodeAndNumber_ReportsCountryCode()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "380",
                    "12a34"
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneCountryCode",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );
        }

        [Fact]
        public void Restore_UnknownCodeAndInvalidNumber_ReportsNationalNumber()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneLogin.Restore(
                    "+999",
                    "12a34"
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "PhoneNationalNumber",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
                );
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void GetEqualityComponents_ValidLogin_ReturnsSinglePhoneComponent()
        {
            var login = PhoneLogin.Restore(
                "+380",
                "981234567"
            );

            var component = Assert.Single(login.GetEqualityComponents());

            Assert.Same(
                login.Phone,
                component
            );
        }

        [Theory]
        [InlineData("+1", "2025550123", "+44", "2025550123")]
        [InlineData("+380", "981234567", "+380", "971234567")]
        public void Equals_DifferentPhones_ReturnsFalse(
            string firstCode,
            string firstNumber,
            string secondCode,
            string secondNumber)
        {
            var first = PhoneLogin.Restore(
                firstCode,
                firstNumber
            );

            var second = PhoneLogin.Restore(
                secondCode,
                secondNumber
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region From and Restore
        [Fact]
        public void FromThenRestore_NormalizedPhone_PreservesEqualityAndHashCode()
        {
            var countryCodeResult = PhoneCountryCode.Create(" +380 ");
            var nationalNumberResult = PhoneNationalNumber.Create(
                "(98) 123-45-67"
            );

            Assert.True(countryCodeResult.IsSuccess);
            Assert.True(nationalNumberResult.IsSuccess);

            var phoneResult = Phone.Create(
                countryCodeResult.Value,
                nationalNumberResult.Value
            );

            Assert.True(phoneResult.IsSuccess);

            var created = Assert.IsType<PhoneLogin>(
                Login.From(phoneResult.Value)
            );

            var restored = PhoneLogin.Restore(
                created.Phone.PhoneCountryCode.Value,
                created.Phone.PhoneNationalNumber.Value
            );

            Assert.Equal(
                "+380",
                created.Phone.PhoneCountryCode.Value
            );

            Assert.Equal(
                "981234567",
                created.Phone.PhoneNationalNumber.Value
            );

            Assert.Equal(
                "+380",
                restored.Phone.PhoneCountryCode.Value
            );

            Assert.Equal(
                "981234567",
                restored.Phone.PhoneNationalNumber.Value
            );

            Assert.Equal(
                "+380981234567",
                created.Value
            );

            Assert.Equal(
                "+380981234567",
                restored.Value
            );

            Assert.Equal(
                LoginType.Phone,
                created.Type
            );

            Assert.Equal(
                LoginType.Phone,
                restored.Type
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
