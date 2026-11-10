using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UnitTests.Common.Models
{
    public class StronglyTypedIdTests
    {
        #region Helpers
        private sealed class TestId<TValue> : StronglyTypedId<TValue>
        {
            public override TValue Value { get; }

            public TestId(TValue value)
            {
                Value = value;
            }
        }

        // Окремий конкретний тип для перевірок рівності.
        // A distinct concrete type used in equality tests.
        private sealed class OtherTestId<TValue> : StronglyTypedId<TValue>
        {
            public override TValue Value { get; }

            public OtherTestId(TValue value)
            {
                Value = value;
            }
        }
        #endregion

        #region GetEqualityComponents
        [Fact]
        public void GetEqualityComponents_GuidValue_ReturnsSingleComponent()
        {
            var value = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var id = new TestId<Guid>(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<Guid>(component);
        }

        [Fact]
        public void GetEqualityComponents_EmptyGuid_ReturnsSingleEmptyGuid()
        {
            var id = new TestId<Guid>(Guid.Empty);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                Guid.Empty,
                component
            );

            Assert.IsType<Guid>(component);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(42)]
        public void GetEqualityComponents_IntegerValue_PreservesValueAndType(int value)
        {
            var id = new TestId<int>(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<int>(component);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void GetEqualityComponents_StringValue_PreservesValue(string value)
        {
            var id = new TestId<string>(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void GetEqualityComponents_NullReferenceValue_ReturnsSingleNullComponent()
        {
            var id = new TestId<string?>(null);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Null(component);
        }

        // Порожній Nullable<T> під час boxing перетворюється на null.
        // Boxing an empty Nullable<T> produces null.
        [Fact]
        public void GetEqualityComponents_NullableValueWithoutValue_ReturnsSingleNullComponent()
        {
            var id = new TestId<int?>(null);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Null(component);
        }

        // Nullable<T> зі значенням боксується як базовий тип T.
        // A populated Nullable<T> boxes as its underlying type T.
        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public void GetEqualityComponents_NullableValueWithValue_ReturnsUnderlyingValue(int value)
        {
            var id = new TestId<int?>(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<int>(component);
        }

        [Fact]
        public void GetEqualityComponents_ReferenceValue_PreservesReference()
        {
            var value = new object();
            var id = new TestId<object>(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Same(
                value,
                component
            );
        }

        [Fact]
        public void GetEqualityComponents_RepeatedEnumeration_ReturnsSameValue()
        {
            var value = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var id = new TestId<Guid>(value);
            var components = id.GetEqualityComponents();

            var first = Assert.Single(components);
            var second = Assert.Single(components);

            Assert.Equal(
                value,
                first
            );

            Assert.Equal(
                value,
                second
            );
        }
        #endregion

        #region Equals
        [Fact]
        public void Equals_SameConcreteTypeAndValue_ReturnsTrue()
        {
            var value = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var first = new TestId<Guid>(value);
            var second = new TestId<Guid>(value);

            Assert.True(first.Equals((object)second));
            Assert.True(second.Equals((object)first));
        }

        [Fact]
        public void Equals_SameConcreteTypeWithDifferentValues_ReturnsFalse()
        {
            var first = new TestId<Guid>(
                Guid.Parse("11111111-1111-1111-1111-111111111111")
            );

            var second = new TestId<Guid>(
                Guid.Parse("22222222-2222-2222-2222-222222222222")
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void Equals_DifferentConcreteTypesWithSameValue_ReturnsFalse()
        {
            var value = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var first = new TestId<Guid>(value);
            var second = new OtherTestId<Guid>(value);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion
    }
}
