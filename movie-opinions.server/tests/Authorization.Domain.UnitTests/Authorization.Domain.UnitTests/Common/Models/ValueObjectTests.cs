using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UnitTests.Common.Models
{
    public class ValueObjectTests
    {
        private class TestValueObject : ValueObject
        {
            private readonly object?[] _components;

            public TestValueObject(params object?[] components)
            {
                _components = components;
            }

            public override IEnumerable<object?> GetEqualityComponents()
                => _components;
        }

        private sealed class DerivedTestValueObject : TestValueObject
        {
            public DerivedTestValueObject(params object?[] components)
                : base(components)
            {
            }
        }

        public static TheoryData<object?[], object?[], bool> EqualityCases => new()
        {
            {
                Array.Empty<object?>(),
                Array.Empty<object?>(),
                true
            },
            { new object?[] { 42 }, new object?[] { 42 }, true },
            { new object?[] { "abc", 42 }, new object?[] { "abc", 42 }, true },
            { new object?[] { "abc", 42 }, new object?[] { "abc", 43 }, false },
            { new object?[] { "abc", 42 }, new object?[] { "other", 42 }, false },
            { new object?[] { "abc", 42 }, new object?[] { 42, "abc" }, false },
            { new object?[] { "abc" }, new object?[] { "abc", 42 }, false },
            { new object?[] { null }, new object?[] { null }, true },
            { new object?[] { null }, new object?[] { "abc" }, false },
            { new object?[] { null, "abc" }, new object?[] { null, "abc" }, true },
            { new object?[] { null, "abc" }, new object?[] { "abc", null }, false },
            { Array.Empty<object?>(), new object?[] { null }, false },
            { new object?[] { 1 }, new object?[] { "1" }, false },
            { new object?[] { 1 }, new object?[] { 1L }, false },
            { new object?[] { "" }, new object?[] { "" }, true },
            { new object?[] { "" }, new object?[] { null }, false },
            { new object?[] { "ABC" }, new object?[] { "abc" }, false }
        };

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualsObject_SameType_ReturnsExpectedResult(
            object?[] firstComponents,
            object?[] secondComponents,
            bool expected)
        {
            var first = new TestValueObject(firstComponents);
            var second = new TestValueObject(secondComponents);

            Assert.Equal(
                expected,
                first.Equals((object)second)
            );

            Assert.Equal(
                expected,
                second.Equals((object)first)
            );
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualsTyped_SameType_ReturnsExpectedResult(
            object?[] firstComponents,
            object?[] secondComponents,
            bool expected)
        {
            var first = new TestValueObject(firstComponents);
            var second = new TestValueObject(secondComponents);

            Assert.Equal(
                expected,
                first.Equals((ValueObject)second)
            );

            Assert.Equal(
                expected,
                second.Equals((ValueObject)first)
            );
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualityOperator_SameType_ReturnsExpectedResult(
            object?[] firstComponents,
            object?[] secondComponents,
            bool expected)
        {
            ValueObject? left = new TestValueObject(firstComponents);
            ValueObject? right = new TestValueObject(secondComponents);

            Assert.Equal(
                expected,
                left == right
            );

            Assert.Equal(
                expected,
                right == left
            );
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void InequalityOperator_SameType_ReturnsExpectedResult(
            object?[] firstComponents,
            object?[] secondComponents,
            bool expected)
        {
            ValueObject? left = new TestValueObject(firstComponents);
            ValueObject? right = new TestValueObject(secondComponents);

            Assert.Equal(
                !expected,
                left != right
            );

            Assert.Equal(
                !expected,
                right != left
            );
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void HashSet_SameType_ReturnsExpectedCount(
            object?[] firstComponents,
            object?[] secondComponents,
            bool expected)
        {
            var set = new HashSet<ValueObject>();

            var firstAdded = set.Add(new TestValueObject(firstComponents));
            var secondAdded = set.Add(new TestValueObject(secondComponents));

            Assert.True(firstAdded);

            Assert.Equal(
                !expected,
                secondAdded
            );

            Assert.Equal(
                expected ? 1 : 2,
                set.Count
            );
        }

        [Fact]
        public void EqualsObject_Null_ReturnsFalse()
        {
            var valueObject = new TestValueObject(42);

            Assert.False(valueObject.Equals((object?)null));
        }

        [Fact]
        public void EqualsObject_NonValueObject_ReturnsFalse()
        {
            var valueObject = new TestValueObject(42);

            Assert.False(valueObject.Equals(new object()));
        }

        [Fact]
        public void EqualsObject_SameInstance_ReturnsTrue()
        {
            var valueObject = new TestValueObject(42);

            Assert.True(valueObject.Equals((object)valueObject));
        }

        [Fact]
        public void EqualsObject_BaseAndDerivedTypesWithSameComponents_ReturnsFalse()
        {
            var baseObject = new TestValueObject("abc", 42);
            var derivedObject = new DerivedTestValueObject("abc", 42);

            Assert.False(baseObject.Equals((object)derivedObject));
            Assert.False(derivedObject.Equals((object)baseObject));
        }

        [Fact]
        public void EqualsObject_EqualNestedValueObjects_ReturnsTrue()
        {
            var firstChild = new TestValueObject(42);
            var secondChild = new TestValueObject(42);

            var first = new TestValueObject(firstChild);
            var second = new TestValueObject(secondChild);

            Assert.True(first.Equals((object)second));
            Assert.True(second.Equals((object)first));
        }

        [Fact]
        public void EqualsObject_DifferentNestedValueObjects_ReturnsFalse()
        {
            var firstChild = new TestValueObject(42);
            var secondChild = new TestValueObject(43);

            var first = new TestValueObject(firstChild);
            var second = new TestValueObject(secondChild);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void EqualsTyped_Null_ReturnsFalse()
        {
            var valueObject = new TestValueObject(42);

            Assert.False(valueObject.Equals((ValueObject?)null));
        }

        [Fact]
        public void EqualsTyped_SameInstance_ReturnsTrue()
        {
            var valueObject = new TestValueObject(42);

            Assert.True(valueObject.Equals((ValueObject)valueObject));
        }

        [Fact]
        public void EqualsTyped_BaseAndDerivedTypesWithSameComponents_ReturnsFalse()
        {
            var baseObject = new TestValueObject("abc", 42);
            var derivedObject = new DerivedTestValueObject("abc", 42);

            Assert.False(baseObject.Equals((ValueObject)derivedObject));
            Assert.False(derivedObject.Equals((ValueObject)baseObject));
        }

        [Fact]
        public void EqualityOperator_BothNull_ReturnsTrue()
        {
            ValueObject? left = null;
            ValueObject? right = null;

            Assert.True(left == right);
        }

        [Fact]
        public void EqualityOperator_LeftNull_ReturnsFalse()
        {
            ValueObject? left = null;
            ValueObject? right = new TestValueObject(42);

            Assert.False(left == right);
        }

        [Fact]
        public void EqualityOperator_RightNull_ReturnsFalse()
        {
            ValueObject? left = new TestValueObject(42);
            ValueObject? right = null;

            Assert.False(left == right);
        }

        [Fact]
        public void EqualityOperator_SameInstance_ReturnsTrue()
        {
            ValueObject? left = new TestValueObject(42);
            ValueObject? right = left;

            Assert.True(left == right);
        }

        [Fact]
        public void EqualityOperator_BaseAndDerivedTypesWithSameComponents_ReturnsFalse()
        {
            ValueObject? left = new TestValueObject("abc", 42);
            ValueObject? right = new DerivedTestValueObject("abc", 42);

            Assert.False(left == right);
            Assert.False(right == left);
        }

        [Fact]
        public void InequalityOperator_BothNull_ReturnsFalse()
        {
            ValueObject? left = null;
            ValueObject? right = null;

            Assert.False(left != right);
        }

        [Fact]
        public void InequalityOperator_LeftNull_ReturnsTrue()
        {
            ValueObject? left = null;
            ValueObject? right = new TestValueObject(42);

            Assert.True(left != right);
        }

        [Fact]
        public void InequalityOperator_RightNull_ReturnsTrue()
        {
            ValueObject? left = new TestValueObject(42);
            ValueObject? right = null;

            Assert.True(left != right);
        }

        [Fact]
        public void InequalityOperator_SameInstance_ReturnsFalse()
        {
            ValueObject? left = new TestValueObject(42);
            ValueObject? right = left;

            Assert.False(left != right);
        }

        [Fact]
        public void InequalityOperator_BaseAndDerivedTypesWithSameComponents_ReturnsTrue()
        {
            ValueObject? left = new TestValueObject("abc", 42);
            ValueObject? right = new DerivedTestValueObject("abc", 42);

            Assert.True(left != right);
            Assert.True(right != left);
        }

        [Fact]
        public void GetHashCode_SameInstance_ReturnsStableValue()
        {
            var valueObject = new TestValueObject("abc", 42, null);

            var first = valueObject.GetHashCode();
            var second = valueObject.GetHashCode();
            var third = valueObject.GetHashCode();

            Assert.Equal(
                first,
                second
            );

            Assert.Equal(
                second,
                third
            );
        }

        [Fact]
        public void GetHashCode_SameTypeAndComponents_ReturnsSameHashCode()
        {
            var first = new TestValueObject("abc", 42, null);
            var second = new TestValueObject("abc", 42, null);

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        [Fact]
        public void GetHashCode_EmptyComponents_ReturnsSameHashCode()
        {
            var first = new TestValueObject();
            var second = new TestValueObject();

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        [Fact]
        public void GetHashCode_NullComponent_ReturnsSameHashCode()
        {
            var first = new TestValueObject(new object?[] { null });
            var second = new TestValueObject(new object?[] { null });

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        [Fact]
        public void GetHashCode_EqualNestedValueObjects_ReturnsSameHashCode()
        {
            var firstChild = new TestValueObject(42);
            var secondChild = new TestValueObject(42);

            var first = new TestValueObject(firstChild);
            var second = new TestValueObject(secondChild);

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        [Fact]
        public void HashSet_DifferentConcreteTypesWithSameComponents_AreStoredSeparately()
        {
            var set = new HashSet<ValueObject>();

            var firstAdded = set.Add(new TestValueObject("abc", 42));
            var secondAdded = set.Add(new DerivedTestValueObject("abc", 42));

            Assert.True(firstAdded);
            Assert.True(secondAdded);

            Assert.Equal(
                2,
                set.Count
            );
        }
    }
}
