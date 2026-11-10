using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.UnitTests.Common.Models
{
    public class EntityTests
    {
        #region Helpers
        private class TestEntity : Entity<Guid>
        {
            public TestEntity(
                Guid id,
                DateTimeOffset? createdAt = null)
                : base(id, createdAt) { }

            public void RegisterDomainEvent(IDomainEvent domainEvent)
                => AddDomainEvent(domainEvent);

            public void RegisterAggregateChange(IAggregateChange aggregateChange)
                => AddAggregateChange(aggregateChange);
        }

        // Окремий конкретний тип для перевірок рівності.
        // A distinct concrete type used in equality tests.
        private sealed class DerivedTestEntity : TestEntity
        {
            public DerivedTestEntity(
                Guid id,
                DateTimeOffset? createdAt = null)
                : base(id, createdAt)
            {
            }
        }

        private sealed class TestDomainEvent : DomainEvent
        {
            public TestDomainEvent(DateTimeOffset occurredOn)
                : base(occurredOn)
            {
            }
        }

        private sealed class TestAggregateChange : AggregateChange
        {
            public TestAggregateChange(DateTimeOffset occurredOn)
                : base(occurredOn)
            {
            }
        }
        #endregion

        #region Constructor
        [Fact]
        public void Constructor_ExplicitArguments_SetsProperties()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.FromHours(2)
            );

            var entity = new TestEntity(
                id,
                createdAt
            );

            Assert.Equal(
                id,
                entity.Id
            );

            Assert.Equal(
                createdAt,
                entity.CreatedAt
            );

            Assert.Equal(
                createdAt.Offset,
                entity.CreatedAt.Offset
            );
        }

        // Поточний час перевіряється в межах вікна до та після виклику.
        // Current time is checked within a before-and-after invocation window.
        [Fact]
        public void Constructor_NullCreatedAt_UsesCurrentUtcTime()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var before = DateTimeOffset.UtcNow;

            var entity = new TestEntity(
                id,
                createdAt: null
            );

            var after = DateTimeOffset.UtcNow;

            Assert.InRange(
                entity.CreatedAt,
                before,
                after
            );

            Assert.Equal(
                TimeSpan.Zero,
                entity.CreatedAt.Offset
            );
        }

        [Fact]
        public void Constructor_NewEntity_HasEmptyEventAndChangeCollections()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var entity = new TestEntity(id);

            Assert.Empty(entity.DomainEvents);

            Assert.Empty(entity.AggregateChanges);
        }
        #endregion

        #region Equals(object)
        [Fact]
        public void EqualsObject_Null_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            Assert.False(entity.Equals((object?)null));
        }

        [Fact]
        public void EqualsObject_NonEntityObject_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            Assert.False(entity.Equals(new object()));
        }

        [Fact]
        public void EqualsObject_SameInstance_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            Assert.True(entity.Equals((object)entity));
        }

        [Fact]
        public void EqualsObject_SameTypeAndId_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var first = new TestEntity(id, createdAt);
            var second = new TestEntity(id, createdAt);

            Assert.True(first.Equals((object)second));
            Assert.True(second.Equals((object)first));
        }

        [Fact]
        public void EqualsObject_SameTypeWithDifferentIds_ReturnsFalse()
        {
            var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var first = new TestEntity(firstId, createdAt);
            var second = new TestEntity(secondId, createdAt);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void EqualsObject_BaseAndDerivedTypesWithSameId_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var baseEntity = new TestEntity(id, createdAt);
            var derivedEntity = new DerivedTestEntity(id, createdAt);

            Assert.False(baseEntity.Equals((object)derivedEntity));
            Assert.False(derivedEntity.Equals((object)baseEntity));
        }

        // Час створення не входить до ідентичності сутності.
        // Creation time is not part of entity identity.
        [Fact]
        public void EqualsObject_SameTypeAndIdWithDifferentCreatedAt_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var firstCreatedAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );
            var secondCreatedAt = firstCreatedAt.AddMinutes(10);

            var first = new TestEntity(id, firstCreatedAt);
            var second = new TestEntity(id, secondCreatedAt);

            Assert.True(first.Equals((object)second));
            Assert.True(second.Equals((object)first));
        }
        #endregion

        #region Typed Equals
        [Fact]
        public void EqualsTyped_Null_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            Assert.False(entity.Equals((Entity<Guid>?)null));
        }

        [Fact]
        public void EqualsTyped_SameInstance_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            Assert.True(entity.Equals((Entity<Guid>)entity));
        }

        [Fact]
        public void EqualsTyped_SameTypeAndId_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var first = new TestEntity(id, createdAt);
            var second = new TestEntity(id, createdAt);

            Assert.True(first.Equals((Entity<Guid>)second));
            Assert.True(second.Equals((Entity<Guid>)first));
        }

        [Fact]
        public void EqualsTyped_SameTypeWithDifferentIds_ReturnsFalse()
        {
            var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var first = new TestEntity(firstId, createdAt);
            var second = new TestEntity(secondId, createdAt);

            Assert.False(first.Equals((Entity<Guid>)second));
            Assert.False(second.Equals((Entity<Guid>)first));
        }

        [Fact]
        public void EqualsTyped_BaseAndDerivedTypesWithSameId_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var baseEntity = new TestEntity(id, createdAt);
            var derivedEntity = new DerivedTestEntity(id, createdAt);

            Assert.False(baseEntity.Equals((Entity<Guid>)derivedEntity));
            Assert.False(derivedEntity.Equals((Entity<Guid>)baseEntity));
        }

        // Час створення не входить до ідентичності сутності.
        // Creation time is not part of entity identity.
        [Fact]
        public void EqualsTyped_SameTypeAndIdWithDifferentCreatedAt_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var firstCreatedAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );
            var secondCreatedAt = firstCreatedAt.AddMinutes(10);

            var first = new TestEntity(id, firstCreatedAt);
            var second = new TestEntity(id, secondCreatedAt);

            Assert.True(first.Equals((Entity<Guid>)second));
            Assert.True(second.Equals((Entity<Guid>)first));
        }
        #endregion

        #region operator ==
        [Fact]
        public void EqualityOperator_BothNull_ReturnsTrue()
        {
            Entity<Guid>? left = null;
            Entity<Guid>? right = null;

            Assert.True(left == right);
        }

        [Fact]
        public void EqualityOperator_LeftNull_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Entity<Guid>? left = null;
            Entity<Guid>? right = new TestEntity(id);

            Assert.False(left == right);
        }

        [Fact]
        public void EqualityOperator_RightNull_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Entity<Guid>? left = new TestEntity(id);
            Entity<Guid>? right = null;

            Assert.False(left == right);
        }

        [Fact]
        public void EqualityOperator_SameInstance_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Entity<Guid>? left = new TestEntity(id);
            Entity<Guid>? right = left;

            Assert.True(left == right);
        }

        [Fact]
        public void EqualityOperator_SameTypeAndId_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            Entity<Guid>? left = new TestEntity(id, createdAt);
            Entity<Guid>? right = new TestEntity(id, createdAt);

            Assert.True(left == right);
            Assert.True(right == left);
        }

        [Fact]
        public void EqualityOperator_SameTypeWithDifferentIds_ReturnsFalse()
        {
            var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            Entity<Guid>? left = new TestEntity(firstId, createdAt);
            Entity<Guid>? right = new TestEntity(secondId, createdAt);

            Assert.False(left == right);
            Assert.False(right == left);
        }

        [Fact]
        public void EqualityOperator_BaseAndDerivedTypesWithSameId_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            Entity<Guid>? left = new TestEntity(id, createdAt);
            Entity<Guid>? right = new DerivedTestEntity(id, createdAt);

            Assert.False(left == right);
            Assert.False(right == left);
        }

        // Час створення не входить до ідентичності сутності.
        // Creation time is not part of entity identity.
        [Fact]
        public void EqualityOperator_SameTypeAndIdWithDifferentCreatedAt_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var firstCreatedAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );
            var secondCreatedAt = firstCreatedAt.AddMinutes(10);

            Entity<Guid>? left = new TestEntity(id, firstCreatedAt);
            Entity<Guid>? right = new TestEntity(id, secondCreatedAt);

            Assert.True(left == right);
            Assert.True(right == left);
        }
        #endregion

        #region operator !=
        [Fact]
        public void InequalityOperator_BothNull_ReturnsFalse()
        {
            Entity<Guid>? left = null;
            Entity<Guid>? right = null;

            Assert.False(left != right);
        }

        [Fact]
        public void InequalityOperator_LeftNull_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Entity<Guid>? left = null;
            Entity<Guid>? right = new TestEntity(id);

            Assert.True(left != right);
        }

        [Fact]
        public void InequalityOperator_RightNull_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Entity<Guid>? left = new TestEntity(id);
            Entity<Guid>? right = null;

            Assert.True(left != right);
        }

        [Fact]
        public void InequalityOperator_SameInstance_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Entity<Guid>? left = new TestEntity(id);
            Entity<Guid>? right = left;

            Assert.False(left != right);
        }

        [Fact]
        public void InequalityOperator_SameTypeAndId_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            Entity<Guid>? left = new TestEntity(id, createdAt);
            Entity<Guid>? right = new TestEntity(id, createdAt);

            Assert.False(left != right);
            Assert.False(right != left);
        }

        [Fact]
        public void InequalityOperator_SameTypeWithDifferentIds_ReturnsTrue()
        {
            var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            Entity<Guid>? left = new TestEntity(firstId, createdAt);
            Entity<Guid>? right = new TestEntity(secondId, createdAt);

            Assert.True(left != right);
            Assert.True(right != left);
        }

        [Fact]
        public void InequalityOperator_BaseAndDerivedTypesWithSameId_ReturnsTrue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            Entity<Guid>? left = new TestEntity(id, createdAt);
            Entity<Guid>? right = new DerivedTestEntity(id, createdAt);

            Assert.True(left != right);
            Assert.True(right != left);
        }

        // Час створення не входить до ідентичності сутності.
        // Creation time is not part of entity identity.
        [Fact]
        public void InequalityOperator_SameTypeAndIdWithDifferentCreatedAt_ReturnsFalse()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var firstCreatedAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );
            var secondCreatedAt = firstCreatedAt.AddMinutes(10);

            Entity<Guid>? left = new TestEntity(id, firstCreatedAt);
            Entity<Guid>? right = new TestEntity(id, secondCreatedAt);

            Assert.False(left != right);
            Assert.False(right != left);
        }
        #endregion

        #region GetHashCode
        [Fact]
        public void GetHashCode_SameInstance_ReturnsStableValue()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var first = entity.GetHashCode();
            var second = entity.GetHashCode();
            var third = entity.GetHashCode();

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
        public void GetHashCode_SameTypeAndId_ReturnsSameHashCode()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var first = new TestEntity(
                id,
                createdAt
            );
            var second = new TestEntity(
                id,
                createdAt
            );

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        // Час створення не входить до ідентичності сутності.
        // Creation time is not part of entity identity.
        [Fact]
        public void GetHashCode_SameTypeAndIdWithDifferentCreatedAt_ReturnsSameHashCode()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var firstCreatedAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );
            var secondCreatedAt = firstCreatedAt.AddMinutes(10);

            var first = new TestEntity(
                id,
                firstCreatedAt
            );
            var second = new TestEntity(
                id,
                secondCreatedAt
            );

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }

        [Fact]
        public void GetHashCode_AfterAddingAndClearingRecords_RemainsStable()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var initialHash = entity.GetHashCode();

            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);
            entity.RegisterAggregateChange(aggregateChange);

            Assert.Equal(
                initialHash,
                entity.GetHashCode()
            );

            entity.ClearDomainEvents();
            entity.ClearAggregateChanges();

            Assert.Equal(
                initialHash,
                entity.GetHashCode()
            );
        }

        [Fact]
        public void HashSet_EqualEntities_AreStoredOnce()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var firstCreatedAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );
            var secondCreatedAt = firstCreatedAt.AddMinutes(10);

            var set = new HashSet<Entity<Guid>>();

            var firstAdded = set.Add(new TestEntity(
                id,
                firstCreatedAt)
            );
            var secondAdded = set.Add(new TestEntity(
                id,
                secondCreatedAt)
            );

            Assert.True(firstAdded);
            Assert.False(secondAdded);
            Assert.Single(set);
        }

        [Fact]
        public void HashSet_DifferentIdentities_AreStoredSeparately()
        {
            var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var createdAt = new DateTimeOffset(
                2026, 10, 30,
                12, 0, 0,
                TimeSpan.Zero
            );

            var set = new HashSet<Entity<Guid>>
            {
                new TestEntity(firstId, createdAt),
                new TestEntity(secondId, createdAt),
                new DerivedTestEntity(firstId, createdAt)
            };

            Assert.Equal(
                3,
                set.Count
            );
        }
        #endregion

        #region DomainEvent
        [Fact]
        public void AddDomainEvent_SingleEvent_PreservesReference()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);

            Assert.Single(entity.DomainEvents);

            Assert.Same(
                domainEvent,
                entity.DomainEvents[0]
            );
        }

        [Fact]
        public void AddDomainEvent_MultipleEvents_PreservesInsertionOrder()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var first = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 30, 0, TimeSpan.Zero)
            );
            var second = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var third = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 15, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(first);
            entity.RegisterDomainEvent(second);
            entity.RegisterDomainEvent(third);

            Assert.Equal(
                3,
                entity.DomainEvents.Count
            );

            Assert.Same(
                first,
                entity.DomainEvents[0]
            );

            Assert.Same(
                second,
                entity.DomainEvents[1]
            );

            Assert.Same(
                third,
                entity.DomainEvents[2]
            );
        }

        [Fact]
        public void AddDomainEvent_SameInstanceTwice_PreservesBothEntries()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);
            entity.RegisterDomainEvent(domainEvent);

            Assert.Equal(
                2,
                entity.DomainEvents.Count
            );

            Assert.Same(
                domainEvent,
                entity.DomainEvents[0]
            );

            Assert.Same(
                domainEvent,
                entity.DomainEvents[1]
            );
        }

        [Fact]
        public void ClearDomainEvents_WithEvents_EmptiesCollection()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            entity.RegisterDomainEvent(
                new TestDomainEvent(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero))
            );
            entity.RegisterDomainEvent(
                new TestDomainEvent(new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero))
            );

            entity.ClearDomainEvents();

            Assert.Empty(entity.DomainEvents);
        }

        [Fact]
        public void ClearDomainEvents_EmptyCollection_DoesNotThrow()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            entity.ClearDomainEvents();
            entity.ClearDomainEvents();

            Assert.Empty(entity.DomainEvents);
        }

        [Fact]
        public void AddDomainEvent_AfterClearing_StoresOnlyNewEvent()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var first = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var second = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(first);
            entity.ClearDomainEvents();
            entity.RegisterDomainEvent(second);

            Assert.Single(entity.DomainEvents);

            Assert.Same(
                second,
                entity.DomainEvents[0]
            );
        }

        [Fact]
        public void DomainEvents_SeparateEntities_HaveIndependentCollections()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var first = new TestEntity(id);
            var second = new TestEntity(id);

            var firstEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var secondEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            first.RegisterDomainEvent(firstEvent);
            second.RegisterDomainEvent(secondEvent);

            Assert.Single(first.DomainEvents);

            Assert.Same(
                firstEvent,
                first.DomainEvents[0]
            );

            Assert.Single(second.DomainEvents);

            Assert.Same(
                secondEvent,
                second.DomainEvents[0]
            );

            first.ClearDomainEvents();

            Assert.Empty(first.DomainEvents);
            Assert.Single(second.DomainEvents);

            Assert.Same(
                secondEvent,
                second.DomainEvents[0]
            );
        }

        // Це живе read-only представлення, а не незалежний знімок.
        // This is a live read-only view rather than an independent snapshot.
        [Fact]
        public void DomainEvents_PreviouslyObtainedView_ReflectsAddAndClear()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var events = entity.DomainEvents;

            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);

            Assert.Single(events);
            Assert.Same(domainEvent, events[0]);

            entity.ClearDomainEvents();

            Assert.Empty(events);
        }

        [Fact]
        public void DomainEvents_ExternalMutation_IsRejected()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var originalEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var anotherEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(originalEvent);

            var list = Assert.IsAssignableFrom<IList<IDomainEvent>>(
                entity.DomainEvents
            );

            Assert.Throws<NotSupportedException>(() =>
                list.Add(anotherEvent)
            );

            Assert.Throws<NotSupportedException>(() =>
            {
                list[0] = anotherEvent;
            });

            Assert.Throws<NotSupportedException>(() =>
                list.Clear()
            );

            Assert.Single(entity.DomainEvents);

            Assert.Same(
                originalEvent,
                entity.DomainEvents[0]
            );
        }
        #endregion

        #region AggregateChange
        [Fact]
        public void AddAggregateChange_SingleChange_PreservesReference()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(aggregateChange);

            Assert.Single(entity.AggregateChanges);

            Assert.Same(
                aggregateChange,
                entity.AggregateChanges[0]
            );
        }

        [Fact]
        public void AddAggregateChange_MultipleChanges_PreservesInsertionOrder()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var first = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 30, 0, TimeSpan.Zero)
            );
            var second = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var third = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 15, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(first);
            entity.RegisterAggregateChange(second);
            entity.RegisterAggregateChange(third);

            Assert.Equal(
                3,
                entity.AggregateChanges.Count
            );

            Assert.Same(
                first,
                entity.AggregateChanges[0]
            );

            Assert.Same(
                second,
                entity.AggregateChanges[1]
            );

            Assert.Same(
                third,
                entity.AggregateChanges[2]
            );
        }

        [Fact]
        public void AddAggregateChange_SameInstanceTwice_PreservesBothEntries()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(aggregateChange);
            entity.RegisterAggregateChange(aggregateChange);

            Assert.Equal(
                2,
                entity.AggregateChanges.Count
            );

            Assert.Same(
                aggregateChange,
                entity.AggregateChanges[0]
            );

            Assert.Same(
                aggregateChange,
                entity.AggregateChanges[1]
            );
        }

        [Fact]
        public void ClearAggregateChanges_WithChanges_EmptiesCollection()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            entity.RegisterAggregateChange(
                new TestAggregateChange(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero))
            );
            entity.RegisterAggregateChange(
                new TestAggregateChange(new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero))
            );

            entity.ClearAggregateChanges();

            Assert.Empty(entity.AggregateChanges);
        }

        [Fact]
        public void ClearAggregateChanges_EmptyCollection_DoesNotThrow()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            entity.ClearAggregateChanges();
            entity.ClearAggregateChanges();

            Assert.Empty(entity.AggregateChanges);
        }

        [Fact]
        public void AddAggregateChange_AfterClearing_StoresOnlyNewChange()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var first = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var second = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(first);
            entity.ClearAggregateChanges();
            entity.RegisterAggregateChange(second);

            Assert.Single(entity.AggregateChanges);

            Assert.Same(
                second,
                entity.AggregateChanges[0]
            );
        }

        [Fact]
        public void AggregateChanges_SeparateEntities_HaveIndependentCollections()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var first = new TestEntity(id);
            var second = new TestEntity(id);

            var firstChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var secondChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            first.RegisterAggregateChange(firstChange);
            second.RegisterAggregateChange(secondChange);

            Assert.Single(first.AggregateChanges);

            Assert.Same(
                firstChange,
                first.AggregateChanges[0]
            );

            Assert.Single(second.AggregateChanges);

            Assert.Same(
                secondChange,
                second.AggregateChanges[0]
            );

            first.ClearAggregateChanges();

            Assert.Empty(first.AggregateChanges);
            Assert.Single(second.AggregateChanges);

            Assert.Same(
                secondChange,
                second.AggregateChanges[0]
            );
        }

        [Fact]
        public void AggregateChanges_PreviouslyObtainedView_ReflectsAddAndClear()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var changes = entity.AggregateChanges;

            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(aggregateChange);

            Assert.Single(changes);

            Assert.Same(
                aggregateChange,
                changes[0]
            );

            entity.ClearAggregateChanges();

            Assert.Empty(changes);
        }

        [Fact]
        public void AggregateChanges_ExternalMutation_IsRejected()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);
            var originalChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var anotherChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(originalChange);

            var list = Assert.IsAssignableFrom<IList<IAggregateChange>>(
                entity.AggregateChanges
            );

            Assert.Throws<NotSupportedException>(() =>
                list.Add(anotherChange)
            );

            Assert.Throws<NotSupportedException>(() =>
            {
                list[0] = anotherChange;
            });

            Assert.Throws<NotSupportedException>(() =>
                list.Clear()
            );

            Assert.Single(entity.AggregateChanges);

            Assert.Same(
                originalChange,
                entity.AggregateChanges[0]
            );
        }
        #endregion

        #region Independence of the two collections
        [Fact]
        public void AddDomainEvent_DoesNotModifyAggregateChanges()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterAggregateChange(aggregateChange);
            entity.RegisterDomainEvent(domainEvent);

            Assert.Single(entity.AggregateChanges);

            Assert.Same(
                aggregateChange,
                entity.AggregateChanges[0]
            );
        }

        [Fact]
        public void AddAggregateChange_DoesNotModifyDomainEvents()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);
            entity.RegisterAggregateChange(aggregateChange);

            Assert.Single(entity.DomainEvents);

            Assert.Same(
                domainEvent,
                entity.DomainEvents[0]
            );
        }

        [Fact]
        public void ClearDomainEvents_DoesNotModifyAggregateChanges()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);
            entity.RegisterAggregateChange(aggregateChange);
            entity.ClearDomainEvents();

            Assert.Empty(entity.DomainEvents);
            Assert.Single(entity.AggregateChanges);

            Assert.Same(
                aggregateChange,
                entity.AggregateChanges[0]
            );
        }

        [Fact]
        public void ClearAggregateChanges_DoesNotModifyDomainEvents()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var entity = new TestEntity(id);

            var domainEvent = new TestDomainEvent(
                new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero)
            );
            var aggregateChange = new TestAggregateChange(
                new DateTimeOffset(2026, 10, 30, 12, 5, 0, TimeSpan.Zero)
            );

            entity.RegisterDomainEvent(domainEvent);
            entity.RegisterAggregateChange(aggregateChange);
            entity.ClearAggregateChanges();

            Assert.Empty(entity.AggregateChanges);
            Assert.Single(entity.DomainEvents);

            Assert.Same(
                domainEvent,
                entity.DomainEvents[0]
            );
        }
        #endregion
    }
}
