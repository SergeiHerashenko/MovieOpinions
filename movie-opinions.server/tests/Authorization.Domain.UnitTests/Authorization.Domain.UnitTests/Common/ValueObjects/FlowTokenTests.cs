using Authorization.Domain.Common.ValueObjects;

namespace Authorization.Domain.UnitTests.Common.ValueObjects
{
    public class FlowTokenTests
    {
        private sealed class TestFlowToken : FlowToken
        {
            public TestFlowToken(string value)
                : base(value) { }

            public static int GetExpectedLength()
                => ExpectedEncodedLength;

            public static string Generate()
                => GenerateValue();

            public static bool CheckLength(string value)
                => HasExpectedLength(value);

            public static bool CheckFormat(string value)
                => HasValidFormat(value);
        }

        private sealed class OtherTestFlowToken : FlowToken
        {
            public OtherTestFlowToken(string value)
                : base(value) { }
        }

        private static string Base64ForBytes(byte fillValue = 0)
        {
            var bytes = new byte[64];

            Array.Fill(
                bytes,
                fillValue
            );

            return Convert.ToBase64String(bytes);
        }

        [Fact]
        public void Constructor_ProvidedValue_SetsValue()
        {
            var value = Base64ForBytes();

            var token = new TestFlowToken(value);

            Assert.Equal(
                value,
                token.Value
            );
        }

        [Fact]
        public void ExpectedEncodedLength_Returns88()
        {
            Assert.Equal(88, TestFlowToken.GetExpectedLength());
        }

        [Fact]
        public void GenerateValue_ReturnsBase64For64Bytes()
        {
            var value = TestFlowToken.Generate();

            Assert.False(string.IsNullOrWhiteSpace(value));
            Assert.Equal(88, value.Length);

            var decoded = Convert.FromBase64String(value);

            Assert.Equal(
                64,
                decoded.Length
            );

            Assert.EndsWith(
                "==",
                value
            );
        }

        [Fact]
        public void HasExpectedLength_Exactly88Characters_ReturnsTrue()
        {
            var value = new string('A', 88);

            Assert.True(TestFlowToken.CheckLength(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(87)]
        [InlineData(89)]
        [InlineData(176)]
        public void HasExpectedLength_OtherLengths_ReturnsFalse(int length)
        {
            var value = new string('A', length);

            Assert.False(TestFlowToken.CheckLength(value));
        }

        [Fact]
        public void HasExpectedLength_InvalidBase64WithCorrectLength_ReturnsTrue()
        {
            var value = new string('!', 88);

            Assert.True(TestFlowToken.CheckLength(value));
        }

        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)251)]
        [InlineData((byte)255)]
        public void HasValidFormat_Base64For64Bytes_ReturnsTrue(byte fillValue)
        {
            var value = Base64ForBytes(fillValue);

            Assert.True(TestFlowToken.CheckFormat(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(32)]
        [InlineData(63)]
        [InlineData(128)]
        public void HasValidFormat_WrongDecodedSize_ReturnsFalse(int byteCount)
        {
            var value = Convert.ToBase64String(new byte[byteCount]);

            Assert.False(TestFlowToken.CheckFormat(value));
        }

        [Theory]
        [InlineData(65)]
        [InlineData(66)]
        public void HasValidFormat_SameEncodedLengthWithWrongDecodedSize_ReturnsFalse(
            int byteCount)
        {
            var value = Convert.ToBase64String(new byte[byteCount]);

            Assert.Equal(
                88,
                value.Length
            );

            Assert.True(TestFlowToken.CheckLength(value));
            Assert.False(TestFlowToken.CheckFormat(value));
        }

        [Theory]
        [InlineData('!')]
        [InlineData('@')]
        [InlineData('-')]
        [InlineData('_')]
        [InlineData('Ж')]
        public void HasValidFormat_InvalidCharacters_ReturnsFalse(char invalidChar)
        {
            var value = Base64ForBytes();
            var chars = value.ToCharArray();
            chars[0] = invalidChar;
            var invalidValue = new string(chars);

            Assert.False(TestFlowToken.CheckFormat(invalidValue));
        }

        [Fact]
        public void HasValidFormat_InvalidPadding_ReturnsFalse()
        {
            var value = Base64ForBytes();

            Assert.False(TestFlowToken.CheckFormat(value[..^1]));
            Assert.False(TestFlowToken.CheckFormat(value[..^2]));
            Assert.False(TestFlowToken.CheckFormat(value + "="));

            var chars = value.ToCharArray();
            chars[0] = '=';
            Assert.False(TestFlowToken.CheckFormat(new string(chars)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(-1)]
        public void HasValidFormat_WhitespaceAroundOrInsideToken_ReturnsTrue(int insertAt)
        {
            var value = Base64ForBytes();
            var whitespace = " \t\r\n";

            var withWhitespace = insertAt switch
            {
                0 => whitespace + value,
                -1 => value + whitespace,
                _ => value.Insert(insertAt, whitespace)
            };

            Assert.True(TestFlowToken.CheckFormat(withWhitespace));
            Assert.False(TestFlowToken.CheckLength(withWhitespace));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void HasValidFormat_OnlyWhitespace_ReturnsFalse(string value)
        {
            Assert.False(TestFlowToken.CheckFormat(value));
        }

        [Fact]
        public void HasValidFormat_OnlyWhitespace_ExactLength_ReturnsFalse()
        {
            var value = new string(' ', 88);

            Assert.False(TestFlowToken.CheckFormat(value));
        }

        [Fact]
        public void GetEqualityComponents_ValidToken_ReturnsSingleStringComponent()
        {
            var value = Base64ForBytes();
            var token = new TestFlowToken(value);

            var component = Assert.Single(token.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_SameConcreteTypeAndValue_ReturnsTrue()
        {
            var value = Base64ForBytes();
            var first = new TestFlowToken(value);
            var second = new TestFlowToken(value);

            Assert.NotSame(
                first,
                second
            );

            Assert.True(first.Equals((object)second));
            Assert.True(second.Equals((object)first));

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        [Fact]
        public void Equals_SameConcreteTypeWithDifferentValues_ReturnsFalse()
        {
            var first = new TestFlowToken(Base64ForBytes(0));
            var second = new TestFlowToken(Base64ForBytes(255));

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void Equals_DifferentConcreteTypesWithSameValue_ReturnsFalse()
        {
            var value = Base64ForBytes();
            var first = new TestFlowToken(value);
            var second = new OtherTestFlowToken(value);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
    }
}
