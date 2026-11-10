using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.PhoneUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.PhoneUser
{
    public class PhoneTests
    {
        #region Create
        [Fact]
        public void Create_NullCountryCode_ThrowsDomainDataInconsistencyException()
        {
            var nationalNumber = PhoneNationalNumber.Restore("981234567");

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Create(
                    null!,
                    nationalNumber
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
                "Phone",
                exception.Context["Type"]
            );

            Assert.Equal(
                "phoneCountryCode",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Create_NullNationalNumber_ThrowsDomainDataInconsistencyException()
        {
            var countryCode = PhoneCountryCode.Restore("+380");

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Create(
                    countryCode,
                    null!
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
                "Phone",
                exception.Context["Type"]
            );

            Assert.Equal(
                "phoneNationalNumber",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Create_BothPartsNull_ReportsCountryCode()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Create(
                    null!,
                    null!
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "phoneCountryCode",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("+380", "981234567")]
        [InlineData("+1", "2025550123")]
        [InlineData("+44", "7700123456")]
        [InlineData("+7", "7012345678")]
        public void Create_ValidParts_SetsProperties(
            string code,
            string number)
        {
            var countryCode = PhoneCountryCode.Restore(code);
            var nationalNumber = PhoneNationalNumber.Restore(number);

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Same(
                countryCode,
                result.Value.PhoneCountryCode
            );

            Assert.Same(
                nationalNumber,
                result.Value.PhoneNationalNumber
            );
        }

        // Один код країни може мати кілька допустимих довжин у каталозі.
        // A shared country code can have several valid lengths in the catalog.
        [Theory]
        [InlineData("1234")]
        [InlineData("12345")]
        public void Create_SharedCountryCode_AcceptsAllCatalogLengths(string number)
        {
            var countryCode = PhoneCountryCode.Restore("+290");
            var nationalNumber = PhoneNationalNumber.Restore(number);

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        public void Create_MultipleAllowedLengths_AcceptsEachLength(int digitCount)
        {
            var countryCode = PhoneCountryCode.Restore("+43");
            var nationalNumber = PhoneNationalNumber.Restore(
                new string('1', digitCount)
            );

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Create_UnknownCountryCode_ReturnsInvalidNumberingPlanFailure()
        {
            var countryCode = PhoneCountryCode.Restore("+999");
            var nationalNumber = PhoneNationalNumber.Restore("123456789");

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PHONE.INVALID_NUMBERING_PLAN",
                error.Code
            );

            Assert.Equal(
                "The country calling code and national number length do not match a supported " +
                "phone numbering plan. Reason: 'UnknownCountryCode'. Owner: 'Phone'.",
                error.Message
            );
        }

        [Fact]
        public void Create_NumberTooShortForCountryCode_ReturnsInvalidNumberingPlanFailure()
        {
            var countryCode = PhoneCountryCode.Restore("+380");
            var nationalNumber = PhoneNationalNumber.Restore("12345678");

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PHONE.INVALID_NUMBERING_PLAN",
                error.Code
            );

            Assert.Contains(
                "NationalNumberTooShort",
                error.Message
            );
        }

        [Fact]
        public void Create_NumberTooLongForCountryCode_ReturnsInvalidNumberingPlanFailure()
        {
            var countryCode = PhoneCountryCode.Restore("+380");
            var nationalNumber = PhoneNationalNumber.Restore("1234567890");

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PHONE.INVALID_NUMBERING_PLAN",
                error.Code
            );

            Assert.Contains(
                "NationalNumberTooLong",
                error.Message
            );
        }

        [Fact]
        public void Create_UnsupportedNumberLength_ReturnsInvalidNumberingPlanFailure()
        {
            var countryCode = PhoneCountryCode.Restore("+231");
            var nationalNumber = PhoneNationalNumber.Restore("12345678");

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PHONE.INVALID_NUMBERING_PLAN",
                error.Code
            );

            Assert.Contains(
                "UnsupportedNationalNumberLength",
                error.Message
            );
        }

        [Theory]
        [InlineData("9")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        [InlineData("8")]
        public void Create_BlockedPrefix_ReturnsNotAllowedFailure(string firstDigit)
        {
            var countryCode = PhoneCountryCode.Restore("+7");
            var nationalNumber = PhoneNationalNumber.Restore(
                firstDigit + "012345678"
            );

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.BusinessRule,
                error.ErrorType
            );

            Assert.Equal(
                "PHONE.NOT_ALLOWED_PHONE",
                error.Code
            );

            Assert.Equal(
                "Phone number is not allowed by the current account policy. Owner: 'Phone'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("6012345678")]
        [InlineData("7012345678")]
        public void Create_UnblockedPrefixForSharedCode_Succeeds(string number)
        {
            var countryCode = PhoneCountryCode.Restore("+7");
            var nationalNumber = PhoneNationalNumber.Restore(number);

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("9")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        [InlineData("8")]
        public void Create_BlockedDigitWithDifferentCountryCode_Succeeds(string firstDigit)
        {
            var countryCode = PhoneCountryCode.Restore("+1");
            var nationalNumber = PhoneNationalNumber.Restore(
                firstDigit + "012345678"
            );

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Create_NumberingPlanViolationAndBlockedPrefix_ReturnsNumberingPlanFailure()
        {
            var countryCode = PhoneCountryCode.Restore("+7");
            var nationalNumber = PhoneNationalNumber.Restore("912345678");

            var result = Phone.Create(
                countryCode,
                nationalNumber
            );

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PHONE.INVALID_NUMBERING_PLAN",
                error.Code
            );

            Assert.Contains(
                "NationalNumberTooShort",
                error.Message
            );
        }

        [Fact]
        public void Create_NormalizedParts_FormsCanonicalPhone()
        {
            var countryCodeResult = PhoneCountryCode.Create(" +380 ");
            var nationalNumberResult = PhoneNationalNumber.Create(
                "(98) 123-45-67"
            );

            Assert.True(countryCodeResult.IsSuccess);
            Assert.True(nationalNumberResult.IsSuccess);

            var result = Phone.Create(
                countryCodeResult.Value,
                nationalNumberResult.Value
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                "+380",
                result.Value.PhoneCountryCode.Value
            );

            Assert.Equal(
                "981234567",
                result.Value.PhoneNationalNumber.Value
            );

            Assert.Equal(
                "+380981234567",
                result.Value.GetFullNumber()
            );
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_NullCountryCode_ThrowsDomainDataInconsistencyException()
        {
            var nationalNumber = PhoneNationalNumber.Restore("981234567");

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Restore(null!, nationalNumber)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "phoneCountryCode",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_NullNationalNumber_ThrowsDomainDataInconsistencyException()
        {
            var countryCode = PhoneCountryCode.Restore("+380");

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Restore(countryCode, null!)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "phoneNationalNumber",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_BothPartsNull_ReportsCountryCode()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Restore(null!, null!)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "phoneCountryCode",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("+380", "981234567")]
        [InlineData("+1", "2025550123")]
        [InlineData("+44", "7700123456")]
        [InlineData("+7", "7012345678")]
        [InlineData("+290", "1234")]
        [InlineData("+290", "12345")]
        public void Restore_ValidParts_SetsProperties(
            string code,
            string number)
        {
            var countryCode = PhoneCountryCode.Restore(code);
            var nationalNumber = PhoneNationalNumber.Restore(number);

            var phone = Phone.Restore(
                countryCode,
                nationalNumber
            );

            Assert.Same(
                countryCode,
                phone.PhoneCountryCode
            );

            Assert.Same(
                nationalNumber,
                phone.PhoneNationalNumber
            );
        }

        [Theory]
        [InlineData("+999", "123456789", "UnknownCountryCode")]
        [InlineData("+380", "12345678", "NationalNumberTooShort")]
        [InlineData("+380", "1234567890", "NationalNumberTooLong")]
        [InlineData("+231", "12345678", "UnsupportedNationalNumberLength")]
        [InlineData("+7", "912345678", "NationalNumberTooShort")]
        public void Restore_InvalidNumberingPlan_ThrowsDomainDataInconsistencyException(
            string code,
            string number,
            string expectedReason)
        {
            var countryCode = PhoneCountryCode.Restore(code);
            var nationalNumber = PhoneNationalNumber.Restore(number);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Phone.Restore(
                    countryCode,
                    nationalNumber
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

        // Історичний номер зберігається без повторного застосування заборони Create.
        // The stored phone is preserved without reapplying the Create restriction.
        [Theory]
        [InlineData("9")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        [InlineData("8")]
        public void Restore_BlockedPrefix_PreservesPhone(string firstDigit)
        {
            var countryCode = PhoneCountryCode.Restore("+7");
            var nationalNumber = PhoneNationalNumber.Restore(
                firstDigit + "012345678"
            );

            var phone = Phone.Restore(
                countryCode,
                nationalNumber
            );

            Assert.Same(
                countryCode,
                phone.PhoneCountryCode
            );

            Assert.Same(
                nationalNumber,
                phone.PhoneNationalNumber
            );
        }
        #endregion

        #region GetFullNumber
        [Theory]
        [InlineData("+380", "981234567", "+380981234567")]
        [InlineData("+1", "2025550123", "+12025550123")]
        [InlineData("+7", "7012345678", "+77012345678")]
        public void GetFullNumber_ValidPhone_ReturnsConcatenatedParts(
            string code,
            string number,
            string expected)
        {
            var phone = Phone.Restore(
                PhoneCountryCode.Restore(code),
                PhoneNationalNumber.Restore(number)
            );

            Assert.Equal(
                expected,
                phone.GetFullNumber()
            );
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void GetEqualityComponents_ValidPhone_ReturnsCodeThenNumber()
        {
            var countryCode = PhoneCountryCode.Restore("+380");
            var nationalNumber = PhoneNationalNumber.Restore("981234567");
            var phone = Phone.Restore(
                countryCode,
                nationalNumber
            );

            var components = phone.GetEqualityComponents().ToArray();

            Assert.Equal(
                2,
                components.Length
            );

            Assert.Same(
                phone.PhoneCountryCode,
                components[0]
            );

            Assert.Same(
                phone.PhoneNationalNumber,
                components[1]
            );
        }

        [Theory]
        [InlineData("+1", "2025550123", "+44", "2025550123")]
        [InlineData("+380", "981234567", "+380", "971234567")]
        public void Equals_OnePartDiffers_ReturnsFalse(
            string firstCode,
            string firstNumber,
            string secondCode,
            string secondNumber)
        {
            var first = Phone.Restore(
                PhoneCountryCode.Restore(firstCode),
                PhoneNationalNumber.Restore(firstNumber)
            );
            var second = Phone.Restore(
                PhoneCountryCode.Restore(secondCode),
                PhoneNationalNumber.Restore(secondNumber)
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region Create and Restore
        [Fact]
        public void CreateThenRestore_ValidPhone_PreservesValuesAndEquality()
        {
            var countryCodeResult = PhoneCountryCode.Create(" +380 ");
            var nationalNumberResult = PhoneNationalNumber.Create(
                "(98) 123-45-67"
            );

            Assert.True(countryCodeResult.IsSuccess);
            Assert.True(nationalNumberResult.IsSuccess);

            var createResult = Phone.Create(
                countryCodeResult.Value,
                nationalNumberResult.Value
            );

            Assert.True(createResult.IsSuccess);
            Assert.Empty(createResult.Errors);

            var created = createResult.Value;
            var restored = Phone.Restore(
                PhoneCountryCode.Restore(created.PhoneCountryCode.Value),
                PhoneNationalNumber.Restore(created.PhoneNationalNumber.Value)
            );

            Assert.Equal(
                "+380",
                created.PhoneCountryCode.Value
            );

            Assert.Equal(
                "981234567",
                created.PhoneNationalNumber.Value
            );

            Assert.Equal(
                "+380",
                restored.PhoneCountryCode.Value
            );

            Assert.Equal(
                "981234567",
                restored.PhoneNationalNumber.Value
            );

            Assert.Equal(
                "+380981234567",
                created.GetFullNumber()
            );

            Assert.Equal(
                "+380981234567",
                restored.GetFullNumber()
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
