using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.Enums;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRestrictionSession
{
    public class UserRestrictionSessionTests
    {
        #region Test data
        // Фіксований час робить перевірки тривалості й часових меж відтворюваними.
        // A fixed timestamp makes duration and time boundary checks reproducible.
        private static readonly DateTimeOffset T0 = new(
            2026, 11, 5, 12, 0, 0, TimeSpan.Zero);
        #endregion

        #region Helpers: factories
        private static UserId User(string suffix = "111111111111")
            => UserId.Restore(Guid.Parse($"11111111-1111-4111-8111-{suffix}"));

        private static UserRestrictionId RestrictionId(byte n)
            => UserRestrictionId.Restore(
                Guid.Parse($"22222222-2222-4222-8222-2222222222{n:D2}"));

        private static UserRestrictionSessionId SessionId()
            => UserRestrictionSessionId.Restore(
                Guid.Parse("33333333-3333-4333-8333-333333333333"));

        private static RestrictionRule Rule(int minutes)
            => RestrictionRule.Restore(
                $"Rule{minutes}",
                minutes
            );

        private static UserRestriction Restriction(
            int minutes,
            UserId? userId = null,
            RestrictionType type = RestrictionType.Ban,
            RestrictionStatus status = RestrictionStatus.Active,
            UserRestrictionId? id = null,
            DateTimeOffset? createdAt = null)
            => UserRestriction.Restore(
                id ?? RestrictionId((byte)(minutes % 100)),
                userId ?? User(),
                Rule(minutes),
                type,
                "System",
                null,
                status,
                createdAt ?? T0,
                status == RestrictionStatus.Revoked ? T0.AddMinutes(1) : null
            );

        private static UserRestrictionSession CreateSession(params UserRestriction[] restrictions)
            => UserRestrictionSession.Create(
                User(),
                restrictions,
                T0
            );

        private static UserRestrictionSession RestoreSession(
            IReadOnlyCollection<UserRestrictionId> ids,
            int totalMinutes,
            RestrictionType type = RestrictionType.Ban,
            DateTimeOffset? createdAt = null,
            UserId? userId = null)
            => UserRestrictionSession.Restore(
                SessionId(),
                userId ?? User(),
                ids,
                type,
                totalMinutes,
                createdAt ?? T0
            );

        private static UserRestriction[] DefaultThree()
            => new[]
            {
                Restriction(
                    10,
                    id: RestrictionId(10)
                ),

                Restriction(
                    20,
                    id: RestrictionId(20)
                ),

                Restriction(
                    30,
                    id: RestrictionId(30)
                )

            };
        #endregion

        #region Helpers: exception assertions
        private static void AssertEmptyField(
            DomainDataInconsistencyException exception,
            string field,
            string operation)
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
                "UserRestrictionSession",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );

            Assert.Equal(
                $"An error occurred during the '{operation}' operation. " +
                $"The field '{field}' in type 'UserRestrictionSession' is empty!",
                exception.Message
            );
        }

        private static void AssertOutOfRange(
            DomainDataInconsistencyException exception,
            string field,
            string operation,
            string? valueType = null)
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
                "UserRestrictionSession",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );

            if (valueType is not null)
            {
                Assert.Equal(
                    valueType,
                    exception.Context["ValueType"]
                );
            }

            Assert.Equal(
                $"An error occurred during the '{operation}' operation. " +
                $"The field '{field}' in type 'UserRestrictionSession' has a value outside the allowed range!",
                exception.Message
            );
        }

        private static void AssertInvariant(
            DomainInvariantViolationException exception,
            string expectedRule,
            string operation)
        {
            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserRestrictionSession",
                exception.Context["Type"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );

            Assert.Equal(
                expectedRule,
                exception.Context["Rule"]
            );

            Assert.Equal(
                $"Critical invariant violation in type 'UserRestrictionSession'. Broken rule: {expectedRule}.",
                exception.Message
            );
        }

        private static void AssertPrecondition(
            DomainInvalidOperationException exception,
            string sourceContext,
            string prerequisite,
            Dictionary<string, object>? custom = null)
        {
            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "UserRestrictionSession",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                sourceContext,
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                prerequisite,
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                $"Precondition failed in '{sourceContext}' for type 'UserRestrictionSession'. " +
                $"Required prerequisite '{prerequisite}' was not satisfied.",
                exception.Message
            );

            if (custom is not null)
            {
                foreach (var (key, value) in custom)
                {
                    Assert.Equal(
                        value,
                        exception.Context[$"Custom_{key}"]
                    );
                }
            }
        }
        #endregion

        #region Helpers: state assertions
        // Копія ідентифікаторів до операції не залежить від подальших змін сесії.
        // The identifier snapshot taken before the operation is independent of later session changes.
        private static void CaptureAndAssertUnchanged(
            UserRestrictionSession session,
            Action act)
        {
            var expectedId = session.Id;
            var expectedUserId = session.UserId;
            var expectedType = session.RestrictionType;
            var expectedMinutes = session.TotalBlockedMinutes;
            var expectedCreatedAt = session.CreatedAt;
            var expectedIds = session.ActiveRestrictionIds.ToArray();

            act();

            Assert.Same(
                expectedId,
                session.Id
            );

            Assert.Same(
                expectedUserId,
                session.UserId
            );

            Assert.Equal(
                expectedType,
                session.RestrictionType
            );

            Assert.Equal(
                expectedMinutes,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                expectedCreatedAt,
                session.CreatedAt
            );

            Assert.Equal(
                expectedIds.Length,
                session.ActiveRestrictionIds.Count
            );

            Assert.True(session.ActiveRestrictionIds.ToHashSet().SetEquals(expectedIds));
            Assert.Empty(session.DomainEvents);
            Assert.Empty(session.AggregateChanges);
        }
        #endregion

        #region Helpers: collection operations
        // Один набір сценаріїв перевіряє спільний контракт валідації та зміни колекції.
        // One scenario set checks the shared contract of collection validation and mutation.
        public enum CollectionOp
        {
            ValidateAdd,
            Add,
            ValidateRemove,
            Remove
        }

        private static void InvokeCollectionOp(
            CollectionOp op,
            UserRestrictionSession session,
            IReadOnlyCollection<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            switch (op)
            {
                case CollectionOp.ValidateAdd:
                    session.ValidateCanAddRestrictions(
                        restrictions,
                        now
                    );
                    break;

                case CollectionOp.Add:
                    session.AddRestrictions(
                        restrictions,
                        now
                    );
                    break;

                case CollectionOp.ValidateRemove:
                    session.ValidateCanRemoveRestrictions(
                        restrictions,
                        now
                    );
                    break;

                case CollectionOp.Remove:
                    session.RemoveRestrictions(
                        restrictions,
                        now
                    );
                    break;
            }
        }
        #endregion

        #region Create: valid state
        [Theory]
        [InlineData(RestrictionType.Ban)]
        [InlineData(RestrictionType.CommentMute)]
        [InlineData(RestrictionType.ChatMute)]
        [InlineData(RestrictionType.AllMute)]
        public void Create_ValidRestrictions_InitializesSession(RestrictionType type)
        {
            var userId = User();

            var items = new[]
            {
                Restriction(
                    10,
                    userId,
                    type,
                    id: RestrictionId(10)
                ),

                Restriction(
                    20,
                    userId,
                    type,
                    id: RestrictionId(20)
                )
            };
            var now = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var session = UserRestrictionSession.Create(
                userId,
                items,
                now
            );

            Assert.Same(
                userId,
                session.UserId
            );

            Assert.Equal(
                type,
                session.RestrictionType
            );

            Assert.Equal(
                30,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                2,
                session.ActiveRestrictionIds.Count
            );

            Assert.Contains(
                RestrictionId(10),
                session.ActiveRestrictionIds
            );

            Assert.Contains(
                RestrictionId(20),
                session.ActiveRestrictionIds
            );

            Assert.NotEqual(
                Guid.Empty,
                session.Id.Value
            );

            Assert.Equal(
                7,
                session.Id.Value.Version
            );

            Assert.Equal(
                now,
                session.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                session.CreatedAt.Offset
            );

            Assert.Empty(session.DomainEvents);
            Assert.Empty(session.AggregateChanges);
        }

        [Fact]
        public void Create_SeparateCalls_GenerateDifferentIds()
        {
            var first = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var second = CreateSession(Restriction(
                10, id: RestrictionId(10))
            );

            Assert.NotEqual(
                first.Id.Value,
                second.Id.Value
            );
        }

        [Fact]
        public void Create_EqualOwnerIdsFromDifferentInstances_Succeeds()
        {
            var sessionOwner = User();

            var restrictionOwner = UserId.Restore(sessionOwner.Value);

            var item = Restriction(
                10,
                restrictionOwner,
                id: RestrictionId(10)
            );

            var session = UserRestrictionSession.Create(
                sessionOwner,
                new[] { item },
                T0
            );

            Assert.Equal(
                10,
                session.TotalBlockedMinutes
            );
        }
        #endregion

        #region Create: required values and collection contents
        [Theory]
        [InlineData(true, false, "userId")]
        [InlineData(false, true, "userRestrictions")]
        [InlineData(true, true, "userId")]
        public void Create_NullRequiredArguments_ThrowsDataInconsistency(
            bool nullUserId,
            bool nullCollection,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Create(
                    nullUserId ? null! : User(),
                    nullCollection ? null! : DefaultThree(),
                    T0
                )
            );

            AssertEmptyField(
                exception,
                expectedField,
                "Create"
            );
        }

        [Fact]
        public void Create_EmptyCollection_ThrowsDataInconsistency()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Create(
                    User(),
                    Array.Empty<UserRestriction>(),
                    T0
                )
            );

            AssertEmptyField(
                exception,
                "userRestrictions",
                "Create"
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Create_NullCollectionItem_ThrowsDataInconsistency(int nullIndex)
        {
            var items = new UserRestriction?[]
            {
                Restriction(
                    10,
                    id: RestrictionId(10)
                ),

                Restriction(
                    20,
                    id: RestrictionId(20)
                ),

                Restriction(
                    30,
                    id: RestrictionId(30)
                )
            };
            items[nullIndex] = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Create(
                    User(),
                    items!,
                    T0
                )
            );

            AssertEmptyField(
                exception,
                "userRestrictions",
                "Create"
            );
        }
        #endregion

        #region Create: owner and restriction consistency
        [Fact]
        public void Create_RestrictionForDifferentUser_ThrowsInvariant()
        {
            var owner = User();

            var foreign = Restriction(
                10,
                User("222222222222"),
                id: RestrictionId(10)
            );

            var local = Restriction(
                20,
                owner,
                id: RestrictionId(20)
            );

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    new[] { local, foreign },
                    T0
                )
            );

            AssertInvariant(
                exception,
                "All restrictions used to create a restriction session must belong to the session owner.",
                "Create"
            );

            Assert.Equal(
                owner.Value,
                exception.Context["State_ExpectedUserId"]
            );

            Assert.Equal(
                foreign.UserId.Value,
                exception.Context["State_ActualUserId"]
            );

            Assert.Equal(
                foreign.Id.Value,
                exception.Context["State_RestrictionId"]
            );
        }

        [Fact]
        public void Create_MixedRestrictionTypes_ThrowsInvariant()
        {
            var owner = User();

            var first = Restriction(
                10,
                owner,
                RestrictionType.Ban,
                id: RestrictionId(10)
            );

            var second = Restriction(
                20,
                owner,
                RestrictionType.ChatMute,
                id: RestrictionId(20)
            );

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Create(owner, new[] { first, second }, T0));

            AssertInvariant(
                exception,
                "All restrictions used to create a restriction session must have the same restriction type.",
                "Create");

            Assert.Equal(
                RestrictionType.Ban,
                exception.Context["State_ExpectedRestrictionType"]
            );

            Assert.Equal(
                RestrictionType.ChatMute,
                exception.Context["State_ActualRestrictionType"]
            );

            Assert.Equal(
                second.Id.Value,
                exception.Context["State_RestrictionId"]
            );
        }

        [Fact]
        public void Create_DuplicateRestrictionIds_ThrowsInvariant()
        {
            var owner = User();
            var id = RestrictionId(10);

            var first = Restriction(
                10,
                owner,
                id: id
            );

            var second = Restriction(
                20,
                owner,
                id: id
            );

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    new[] { first, second },
                    T0
                )
            );

            AssertInvariant(
                exception,
                "A restriction session cannot contain duplicate restriction identifiers.",
                "Create"
            );

            Assert.Equal(
                owner.Value,
                exception.Context["State_UserId"]
            );

            Assert.Equal(
                2,
                exception.Context["State_TotalCount"]
            );
        }
        #endregion

        #region Create: duration limits
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_TotalDurationAtInt32Limit_Succeeds(bool split)
        {
            var owner = User();

            UserRestriction[] items = split
                ? new[]
                {
                    Restriction(
                        int.MaxValue - 1,
                        owner,
                        id: RestrictionId(1)
                    ),

                    Restriction(
                        1,
                        owner,
                        id: RestrictionId(2)
                    )
                }
                : new[]
                {
                    Restriction(
                        int.MaxValue,
                        owner,
                        id: RestrictionId(1)
                    )
                };

            var session = UserRestrictionSession.Create(
                owner,
                items,
                T0
            );

            Assert.Equal(
                int.MaxValue,
                session.TotalBlockedMinutes
            );
        }

        [Fact]
        public void Create_TotalDurationOverflow_ThrowsOverflowException()
        {
            var owner = User();
            var items = new[]
            {
                Restriction(
                    int.MaxValue,
                    owner,
                    id: RestrictionId(1)
                ),

                Restriction(
                    1,
                    owner,
                    id: RestrictionId(2)
                )
            };

            Assert.Throws<OverflowException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    items,
                    T0
                )
            );
        }
        #endregion

        #region Create: input collection independence
        [Fact]
        public void Create_SourceCollectionChanges_DoNotChangeSession()
        {
            var items = DefaultThree().ToList();

            var session = UserRestrictionSession.Create(
                User(),
                items,
                T0
            );

            items.Clear();

            Assert.Equal(
                3,
                session.ActiveRestrictionIds.Count
            );

            Assert.Equal(
                60,
                session.TotalBlockedMinutes
            );
        }

        // Локальний ітератор рахує фактичні проходи вхідною послідовністю.
        // The local iterator counts actual enumerations of the input sequence.
        [Fact]
        public void Create_EnumerableInput_IsEnumeratedOnce()
        {
            var callCount = 0;

            IEnumerable<UserRestriction> Once()
            {
                callCount++;
                yield return Restriction(10, id: RestrictionId(10));
            }

            var session = UserRestrictionSession.Create(
                User(),
                Once(),
                T0
            );

            Assert.Equal(
                1,
                callCount
            );

            Assert.Equal(
                10,
                session.TotalBlockedMinutes
            );
        }
        #endregion

        #region Create: validation order
        [Fact]
        public void Create_ValidationOrder_ReportsFirstViolation()
        {
            var owner = User();

            var itemsWithNull = new UserRestriction?[]
            {
                Restriction(
                    10,
                    User("222222222222"),
                    id: RestrictionId(10)
                ),
                null
            };

            var nullItem = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    itemsWithNull!,
                    T0
                )
            );

            AssertEmptyField(
                nullItem,
                "userRestrictions",
                "Create"
            );

            var local = Restriction(
                10,
                owner,
                RestrictionType.Ban,
                id: RestrictionId(10)
            );

            var foreign = Restriction(
                20,
                User("222222222222"),
                RestrictionType.ChatMute,
                id: RestrictionId(20)
            );

            var ownerFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    new[] { local, foreign },
                    T0
                )
            );

            Assert.Contains(
                "must belong to the session owner",
                ownerFirst.Context["Rule"]!.ToString()
            );

            var a = Restriction(
                10,
                owner,
                RestrictionType.Ban,
                id: RestrictionId(10)
            );

            var b = Restriction(
                20,
                owner,
                RestrictionType.ChatMute,
                id: RestrictionId(10)
            );

            var typeBeforeDup = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    new[] { a, b },
                    T0
                )
            );

            Assert.Contains(
                "must have the same restriction type",
                typeBeforeDup.Context["Rule"]!.ToString()
            );

            var d1 = Restriction(
                int.MaxValue,
                owner,
                id: RestrictionId(1)
            );

            var d2 = Restriction(
                1,
                owner,
                id: RestrictionId(1)
            );

            var dupBeforeOverflow = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Create(
                    owner,
                    new[] { d1, d2 },
                    T0
                )
            );

            Assert.Contains(
                "cannot contain duplicate restriction identifiers",
                dupBeforeOverflow.Context["Rule"]!.ToString()
            );
        }
        #endregion

        #region Restore
        [Theory]
        [InlineData(RestrictionType.Ban, 1)]
        [InlineData(RestrictionType.ChatMute, 60)]
        [InlineData(RestrictionType.AllMute, int.MaxValue)]
        public void Restore_ValidArguments_PreservesProperties(
            RestrictionType type,
            int minutes)
        {
            var id = SessionId();
            var userId = User();

            var restrictionIds = new[] {
                RestrictionId(10),
                RestrictionId(20)
            };

            var created = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var session = UserRestrictionSession.Restore(
                id,
                userId,
                restrictionIds,
                type,
                minutes,
                created
            );

            Assert.Same(
                id,
                session.Id
            );

            Assert.Same(
                userId,
                session.UserId
            );

            Assert.Equal(
                type,
                session.RestrictionType
            );

            Assert.Equal(
                minutes,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                2,
                session.ActiveRestrictionIds.Count
            );

            Assert.True(session.ActiveRestrictionIds.ToHashSet().SetEquals(restrictionIds));

            Assert.Equal(
                created,
                session.CreatedAt
            );

            Assert.Equal(
                created.Offset,
                session.CreatedAt.Offset
            );

            Assert.Empty(session.DomainEvents);
            Assert.Empty(session.AggregateChanges);
        }

        [Theory]
        [InlineData(true, false, false, "userRestrictionSessionId")]
        [InlineData(false, true, false, "userId")]
        [InlineData(false, false, true, "userRestrictionIds")]
        [InlineData(true, true, true, "userRestrictionSessionId")]
        public void Restore_NullRequiredArguments_ThrowsDataInconsistency(
            bool nullSessionId,
            bool nullUserId,
            bool nullIds,
            string expectedField)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    nullSessionId ? null! : SessionId(),
                    nullUserId ? null! : User(),
                    nullIds ? null! : new[] { RestrictionId(10) },
                    RestrictionType.Ban,
                    10,
                    T0
                )
            );

            AssertEmptyField(
                exception,
                expectedField,
                "Restore"
            );
        }

        [Fact]
        public void Restore_EmptyIdCollection_ThrowsDataInconsistency()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    Array.Empty<UserRestrictionId>(),
                    RestrictionType.Ban,
                    10,
                    T0
                )
            );

            AssertEmptyField(
                exception,
                "userRestrictionIds",
                "Restore"
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Restore_NullIdCollectionItem_ThrowsDataInconsistency(int nullIndex)
        {
            var ids = new UserRestrictionId?[]
            {
                RestrictionId(10),
                RestrictionId(20)
            };

            ids[nullIndex] = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    ids!,
                    RestrictionType.Ban,
                    10,
                    T0
                )
            );

            AssertEmptyField(
                exception,
                "userRestrictionIds",
                "Restore"
            );
        }

        [Fact]
        public void Restore_DuplicateIds_ThrowsInvariant()
        {
            var id = RestrictionId(10);
            var ids = new[] { id, UserRestrictionId.Restore(id.Value) };

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    ids,
                    RestrictionType.Ban,
                    10,
                    T0
                )
            );

            AssertInvariant(
                exception,
                "A restored restriction session cannot contain duplicate restriction identifiers.",
                "Restore"
            );

            Assert.Equal(
                2,
                exception.Context["State_TotalRestrictionIdCount"]
            );

            Assert.Equal(
                1,
                exception.Context["State_UniqueRestrictionIdCount"]
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Restore_UndefinedRestrictionType_ThrowsDataInconsistency(int typeValue)
        {
            var type = (RestrictionType)typeValue;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    new[] { RestrictionId(10) },
                    type,
                    10,
                    T0
                )
            );

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
                "UserRestrictionSession",
                exception.Context["Type"]
            );

            Assert.Equal(
                "restrictionType",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                type,
                exception.Context["Value"]
            );

            Assert.Equal(
                $"An error occurred during the 'Restore' operation. " +
                $"Unsupported discriminator value '{type}' for type 'UserRestrictionSession'!",
                exception.Message
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Restore_NonPositiveTotalDuration_ThrowsDataInconsistency(int minutes)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    new[] { RestrictionId(10) },
                    RestrictionType.Ban,
                    minutes,
                    T0
                )
            );

            AssertOutOfRange(
                exception,
                "totalBlockedMinutes",
                "Restore",
                "Int32"
            );
        }

        [Fact]
        public void Restore_SourceCollectionChanges_DoNotChangeSession()
        {
            var ids = new List<UserRestrictionId> { RestrictionId(10) };

            var session = UserRestrictionSession.Restore(
                SessionId(),
                User(),
                ids,
                RestrictionType.Ban,
                10,
                T0
            );

            ids.Clear();

            Assert.Single(session.ActiveRestrictionIds);
        }

        [Fact]
        public void Restore_ValidationOrder_ReportsFirstViolation()
        {
            var idsWithNull = new UserRestrictionId?[]
            {
                RestrictionId(10),
                null,
                RestrictionId(10)
            };

            var nullItem = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    idsWithNull!,
                    RestrictionType.Ban,
                    0,
                    T0
                )
            );

            AssertEmptyField(
                nullItem,
                "userRestrictionIds",
                "Restore"
            );

            var dupBeforeEnum = Assert.Throws<DomainInvariantViolationException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    new[] {
                        RestrictionId(10),
                        RestrictionId(10)
                    },
                    (RestrictionType)999,
                    0,
                    T0
                )
            );

            Assert.Contains(
                "duplicate restriction identifiers",
                dupBeforeEnum.Context["Rule"]!.ToString()
            );

            var typeBeforeDuration = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSession.Restore(
                    SessionId(),
                    User(),
                    new[] { RestrictionId(10) },
                    (RestrictionType)999,
                    0,
                    T0
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                typeBeforeDuration.ExceptionCode
            );

            Assert.Equal(
                "restrictionType",
                typeBeforeDuration.Context["Field"]
            );
        }
        #endregion

        #region Time queries and snapshots
        [Theory]
        [InlineData(1)]
        [InlineData(60)]
        public void GetExpirationDate_ValidDuration_AddsMinutesToCreatedAt(int minutes)
        {
            var created = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.FromHours(2));

            var session = RestoreSession(
                new[] { RestrictionId(10) },
                minutes,
                createdAt: created
            );

            var expiration = session.GetExpirationDate();

            Assert.Equal(
                created.AddMinutes(minutes),
                expiration
            );

            Assert.Equal(
                created.Offset,
                expiration.Offset
            );
        }

        [Fact]
        public void IsActive_TimeBoundaries_ReturnsExpectedResult()
        {
            var session = RestoreSession(new[] { RestrictionId(10) }, 60);

            Assert.False(session.IsActive(T0.AddTicks(-1)));
            Assert.True(session.IsActive(T0));
            Assert.True(session.IsActive(T0.AddMinutes(60).AddTicks(-1)));
            Assert.False(session.IsActive(T0.AddMinutes(60)));
            Assert.False(session.IsActive(T0.AddMinutes(60).AddTicks(1)));
        }

        [Fact]
        public void GetRemainingTime_TimeBoundaries_ReturnsExpectedDuration()
        {
            var session = RestoreSession(new[] { RestrictionId(10) }, 60);
            var expiry = T0.AddMinutes(60);

            Assert.Equal(
                TimeSpan.Zero,
                session.GetRemainingTime(T0.AddTicks(-1))
            );

            Assert.Equal(
                TimeSpan.FromMinutes(60),
                session.GetRemainingTime(T0)
            );

            Assert.Equal(
                TimeSpan.FromTicks(1),
                session.GetRemainingTime(expiry.AddTicks(-1))
            );

            Assert.Equal(
                TimeSpan.Zero,
                session.GetRemainingTime(expiry)
            );

            Assert.Equal(
                TimeSpan.Zero,
                session.GetRemainingTime(expiry.AddTicks(1))
            );
        }

        [Fact]
        public void IsExpiredAt_BeforeCreation_ThrowsDataInconsistency()
        {
            var session = RestoreSession(new[] { RestrictionId(10) }, 60);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                session.IsExpiredAt(T0.AddTicks(-1)));

            AssertOutOfRange(
                exception,
                "now",
                "Read",
                "DateTimeOffset"
            );

            Assert.Equal(
                "CreatedAt",
                exception.Context["Custom_MinimumField"]
            );

            Assert.Equal(
                T0,
                exception.Context["Custom_MinimumValue"]
            );
        }

        [Fact]
        public void IsExpiredAt_AtOrAfterCreation_ReturnsExpectedResult()
        {
            var session = RestoreSession(new[] { RestrictionId(10) }, 60);
            var expiry = T0.AddMinutes(60);

            Assert.False(session.IsExpiredAt(T0));
            Assert.False(session.IsExpiredAt(expiry.AddTicks(-1)));
            Assert.True(session.IsExpiredAt(expiry));
            Assert.True(session.IsExpiredAt(expiry.AddTicks(1)));
        }

        [Fact]
        public void Queries_DoNotMutateSession()
        {
            var session = RestoreSession(new[] { RestrictionId(10) }, 60);

            CaptureAndAssertUnchanged(session, () =>
            {
                _ = session.GetExpirationDate();
                _ = session.IsActive(T0);
                _ = session.GetRemainingTime(T0);
                _ = session.IsExpiredAt(T0);
            });
        }

        // Попередні знімки ідентифікаторів зберігають свій склад після зміни сесії.
        // Earlier identifier snapshots retain their contents after the session changes.
        [Fact]
        public void ActiveRestrictionIds_ReturnsIndependentSnapshots()
        {
            var session = CreateSession(
                Restriction(
                    10,
                    id: RestrictionId(10)
                ),

                Restriction(
                    20,
                    id: RestrictionId(20)
                )
            );

            var snapshot = session.ActiveRestrictionIds;

            Assert.Equal(
                2,
                snapshot.Count
            );

            session.AddRestrictions(
                new[] { Restriction(
                    30,
                    id: RestrictionId(30))
                },
                T0
            );

            Assert.Equal(
                2,
                snapshot.Count
            );

            Assert.Equal(
                3,
                session.ActiveRestrictionIds.Count
            );

            var afterAdd = session.ActiveRestrictionIds;

            session.RemoveRestrictions(
                new[] { Restriction(
                    10,
                    id: RestrictionId(10))
                },
                T0
            );

            Assert.Equal(
                3,
                afterAdd.Count
            );

            Assert.Equal(
                2,
                session.ActiveRestrictionIds.Count
            );
        }

        // Результат залежить від того, чи дійде запит до обчислення дати завершення.
        // The outcome depends on whether the query reaches the expiration date calculation.
        [Fact]
        public void Queries_ExpirationDateOverflow_ObserveCurrentBehavior()
        {
            var session = RestoreSession(
                new[] { RestrictionId(10) },
                1,
                createdAt: DateTimeOffset.MaxValue
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                session.GetExpirationDate());

            Assert.False(session.IsActive(DateTimeOffset.MaxValue.AddTicks(-1)));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                session.IsActive(DateTimeOffset.MaxValue));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                session.GetRemainingTime(DateTimeOffset.MaxValue.AddTicks(-1)));

            Assert.Throws<DomainDataInconsistencyException>(() =>
                session.IsExpiredAt(DateTimeOffset.MaxValue.AddTicks(-1)));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                session.IsExpiredAt(DateTimeOffset.MaxValue));
        }
        #endregion

        #region ContainsExactly
        [Fact]
        public void ContainsExactly_SameIdsInDifferentOrder_ReturnsTrue()
        {
            var session = CreateSession(DefaultThree());

            var shuffled = new[]
            {
                RestrictionId(30),
                RestrictionId(10),
                RestrictionId(20)
            };

            var restored = new[]
            {
                UserRestrictionId.Restore(RestrictionId(10).Value),
                UserRestrictionId.Restore(RestrictionId(20).Value),
                UserRestrictionId.Restore(RestrictionId(30).Value)
            };

            CaptureAndAssertUnchanged(session, () =>
            {
                Assert.True(session.ContainsExactly(shuffled));
                Assert.True(session.ContainsExactly(restored));
            });
        }

        [Fact]
        public void ContainsExactly_MissingExtraOrDifferentIds_ReturnsFalse()
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                Assert.False(session.ContainsExactly(Array.Empty<UserRestrictionId>()));
                Assert.False(session.ContainsExactly(new[] { RestrictionId(10) }));

                Assert.False(session.ContainsExactly(new[]
                {
                    RestrictionId(10),
                    RestrictionId(20),
                    RestrictionId(30),
                    RestrictionId(40)
                }));

                Assert.False(session.ContainsExactly(new[]
                {
                    RestrictionId(10),
                    RestrictionId(20),
                    RestrictionId(40)
                }));
            });
        }

        [Fact]
        public void ContainsExactly_DuplicateInputIds_ReturnsFalse()
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                Assert.False(session.ContainsExactly(new[]
                {
                    RestrictionId(10),
                    RestrictionId(10),
                    RestrictionId(20)
                }));

                Assert.False(session.ContainsExactly(new[]
                {
                    RestrictionId(10),
                    RestrictionId(20),
                    RestrictionId(30),
                    RestrictionId(10)
                }));
            });
        }

        [Fact]
        public void ContainsExactly_NullCollection_ThrowsDataInconsistency()
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    session.ContainsExactly(null!));

                AssertEmptyField(
                    exception,
                    "restrictionIds",
                    "Read"
                );
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ContainsExactly_NullItem_ThrowsInvariant(int nullIndex)
        {
            var session = CreateSession(DefaultThree());
            var ids = new UserRestrictionId?[]
            {
                RestrictionId(10),
                RestrictionId(20)
            };
            ids[nullIndex] = null;

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    session.ContainsExactly(ids!));

                AssertInvariant(
                    exception,
                    "The restriction identifier collection contains a null value.",
                    "Read"
                );

                Assert.Equal(
                    2,
                    exception.Context["State_RestrictionIdentifierCount"]
                );
            });
        }

        [Fact]
        public void ContainsExactly_EmptySessionAndEmptyInput_ReturnsTrue()
        {
            var items = DefaultThree();
            var session = CreateSession(items);
            session.RemoveRestrictions(items, T0);

            CaptureAndAssertUnchanged(session, () =>
                Assert.True(session.ContainsExactly(Array.Empty<UserRestrictionId>())));
        }
        #endregion

        #region Test data: collection operations
        public static TheoryData<CollectionOp> AllCollectionOps()
            => new()
            {
                CollectionOp.ValidateAdd,
                CollectionOp.Add,
                CollectionOp.ValidateRemove,
                CollectionOp.Remove
            };
        #endregion

        #region Collection operations: common validation
        [Theory]
        [MemberData(nameof(AllCollectionOps))]
        public void CollectionOperations_NullCollection_ThrowsDataInconsistency(CollectionOp op)
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    InvokeCollectionOp(op, session, null!, T0));

                AssertEmptyField(
                    exception,
                    "userRestrictions",
                    "Update"
                );
            });
        }

        [Theory]
        [MemberData(nameof(AllCollectionOps))]
        public void CollectionOperations_BeforeCreation_ThrowsDataInconsistency(CollectionOp op)
        {
            var session = CreateSession(DefaultThree());
            var batch = new[] {
                Restriction(
                    5,
                    id: RestrictionId(5)
                )
            };

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                    InvokeCollectionOp(
                        op,
                        session,
                        batch,
                        T0.AddTicks(-1)
                    )
                );

                AssertOutOfRange(
                    exception,
                    "now",
                    "Update",
                    "DateTimeOffset"
                );

                Assert.Equal(
                    "CreatedAt",
                    exception.Context["Custom_MinimumField"]
                );

                Assert.Equal(
                    T0,
                    exception.Context["Custom_MinimumValue"]
                );
            });
        }

        [Theory]
        [MemberData(nameof(AllCollectionOps))]
        public void CollectionOperations_ValidationOrder_NullBeforeTime(CollectionOp op)
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                var nullFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                    InvokeCollectionOp(
                        op,
                        session,
                        null!,
                        T0.AddTicks(-1)
                    )
                );

                AssertEmptyField(
                    nullFirst,
                    "userRestrictions",
                    "Update"
                );
            });
        }

        [Theory]
        [MemberData(nameof(AllCollectionOps))]
        public void CollectionOperations_ValidationOrder_TimeBeforeEmpty(CollectionOp op)
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                var timeFirst = Assert.Throws<DomainDataInconsistencyException>(() =>
                    InvokeCollectionOp(
                        op,
                        session,
                        Array.Empty<UserRestriction>(),
                        T0.AddTicks(-1)
                    )
                );

                AssertOutOfRange(
                    timeFirst,
                    "now",
                    "Update",
                    "DateTimeOffset"
                );
            });
        }

        [Theory]
        [InlineData(CollectionOp.ValidateAdd, "PrepareAddition")]
        [InlineData(CollectionOp.Add, "PrepareAddition")]
        [InlineData(CollectionOp.ValidateRemove, "PrepareRemoval")]
        [InlineData(CollectionOp.Remove, "PrepareRemoval")]
        public void CollectionOperations_EmptyCollection_ThrowsPreconditionFailed(
            CollectionOp op,
            string sourceContext)
        {
            var session = CreateSession(DefaultThree());

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                    InvokeCollectionOp(
                        op,
                        session,
                        Array.Empty<UserRestriction>(),
                        T0
                    )
                );

                AssertPrecondition(
                    exception,
                    sourceContext,
                    "A non-empty restriction collection.",
                    new Dictionary<string, object> { ["RestrictionCount"] = 0 }
                );
            });
        }

        [Theory]
        [InlineData(CollectionOp.ValidateAdd, "PrepareAddition")]
        [InlineData(CollectionOp.Add, "PrepareAddition")]
        [InlineData(CollectionOp.ValidateRemove, "PrepareRemoval")]
        [InlineData(CollectionOp.Remove, "PrepareRemoval")]
        public void CollectionOperations_NullItem_ThrowsPreconditionFailed(
            CollectionOp op,
            string sourceContext)
        {
            var session = CreateSession(DefaultThree());
            var items = new UserRestriction?[]
            {
                Restriction(
                    10,
                    id: RestrictionId(10)
                ),
                null
            };

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                    InvokeCollectionOp(
                        op,
                        session,
                        items!,
                        T0
                    )
                );

                AssertPrecondition(
                    exception,
                    sourceContext,
                    "All restrictions must be non-null.",
                    new Dictionary<string, object>
                    {
                        ["RestrictionCount"] = 2,
                        ["NullRestrictionCount"] = 1
                    }
                );
            });
        }
        #endregion

        #region Adding restrictions: validation
        [Theory]
        [InlineData(true, RestrictionStatus.Completed)]
        [InlineData(true, RestrictionStatus.Revoked)]
        [InlineData(false, RestrictionStatus.Completed)]
        [InlineData(false, RestrictionStatus.Revoked)]
        public void ValidateOrAdd_NonActiveRestriction_ThrowsInvariant(
            bool validateOnly,
            RestrictionStatus status)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var inactive = Restriction(
                5,
                status: status,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { inactive },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { inactive },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "Only active restrictions can be added to an active restriction session.",
                    "Update"
                );

                Assert.Equal(
                    inactive.Id.Value,
                    exception.Context["State_RestrictionId"]
                );

                Assert.Equal(
                    status,
                    exception.Context["State_RestrictionStatus"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_DifferentOwner_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var foreign = Restriction(
                5,
                User("222222222222"),
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { foreign },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { foreign },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "A restriction belonging to another user cannot be added to this restriction session.",
                    "Update"
                );

                Assert.Equal(
                    session.UserId.Value,
                    exception.Context["State_ExpectedUserId"]
                );

                Assert.Equal(
                    foreign.UserId.Value,
                    exception.Context["State_ActualUserId"]
                );
            });
        }

        [Fact]
        public void ValidateOrAdd_SameOwnerDifferentInstance_Succeeds()
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var sameGuidOwner = UserId.Restore(User().Value);

            var item = Restriction(
                5,
                sameGuidOwner,
                id: RestrictionId(5)
            );

            session.ValidateCanAddRestrictions(
                new[] { item },
                T0
            );

            session.AddRestrictions(
                new[] { item },
                T0
            );

            Assert.Equal(
                15,
                session.TotalBlockedMinutes
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_DifferentRestrictionType_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var other = Restriction(
                5,
                type: RestrictionType.ChatMute,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { other },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { other },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "A restriction with a different type cannot be added to this restriction session.",
                    "Update"
                );

                Assert.Equal(
                    RestrictionType.Ban,
                    exception.Context["State_ExpectedRestrictionType"]
                );

                Assert.Equal(
                    RestrictionType.ChatMute,
                    exception.Context["State_ActualRestrictionType"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_AlreadyIncludedId_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var sameId = Restriction(
                5,
                id: RestrictionId(10)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { sameId },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { sameId },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "One or more restrictions are already included in the session.",
                    "Update"
                );

                Assert.Equal(
                    session.Id.Value,
                    exception.Context["State_SessionId"]
                );

                Assert.Equal(
                    sameId.Id.Value,
                    exception.Context["State_RestrictionId"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_DuplicateIncomingIds_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var a = Restriction(
                5,
                id: RestrictionId(5)
            );

            var b = Restriction(
                7,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { a, b },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { a, b },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "Restrictions being added contain duplicate identifiers.",
                    "Update"
                );

                Assert.Equal(
                    session.Id.Value,
                    exception.Context["State_SessionId"]
                );

                Assert.Equal(
                    a.Id.Value,
                    exception.Context["State_DuplicateRestrictionId"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_ValidationOrder_ReportsFirstViolation(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var inactiveForeign = Restriction(
                5,
                User("222222222222"),
                status: RestrictionStatus.Completed,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var statusFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { inactiveForeign },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { inactiveForeign },
                            T0
                        );
                    }
                });

                Assert.Contains(
                    "Only active restrictions",
                    statusFirst.Context["Rule"]!.ToString()
                );
            });

            var foreignOtherType = Restriction(
                5,
                User("222222222222"),
                RestrictionType.ChatMute,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var ownerFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { foreignOtherType },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { foreignOtherType },
                            T0
                        );
                    }
                });

                Assert.Contains(
                    "belonging to another user",
                    ownerFirst.Context["Rule"]!.ToString()
                );
            });

            var sameIdOtherType = Restriction(
                5,
                type: RestrictionType.ChatMute,
                id: RestrictionId(10)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var typeFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { sameIdOtherType },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { sameIdOtherType },
                            T0
                        );
                    }
                });

                Assert.Contains(
                    "different type",
                    typeFirst.Context["Rule"]!.ToString()
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_IncomingDurationOverflow_ThrowsOverflowException(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var items = new[]
            {
                Restriction(
                    int.MaxValue,
                    id: RestrictionId(1)
                ),

                Restriction(
                    1,
                    id: RestrictionId(2)
                )
            };

            CaptureAndAssertUnchanged(session, () =>
            {
                Assert.Throws<OverflowException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            items,
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            items,
                            T0
                        );
                    }
                });
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrAdd_UpdatedTotalOverflow_ThrowsOverflowException(bool validateOnly)
        {
            var session = RestoreSession(
                new[] {
                    RestrictionId(10)
                },
                int.MaxValue
            );

            var item = Restriction(
                1,
                id: RestrictionId(1)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                Assert.Throws<OverflowException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanAddRestrictions(
                            new[] { item },
                            T0
                        );
                    }
                    else
                    {
                        session.AddRestrictions(
                            new[] { item },
                            T0
                        );
                    }
                });
            });
        }
        #endregion

        #region ValidateCanAddRestrictions
        [Fact]
        public void ValidateCanAddRestrictions_ValidBatch_DoesNotMutateSession()
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var batch = new[]
            {
                Restriction(
                    5,
                    id: RestrictionId(5)
                ),

                Restriction(
                    7,
                    id: RestrictionId(7)
                )
            };

            CaptureAndAssertUnchanged(session, () =>
                session.ValidateCanAddRestrictions(
                    batch,
                    T0
                )
            );
        }
        #endregion

        #region AddRestrictions
        [Fact]
        public void AddRestrictions_ValidBatch_AddsIdsAndDuration()
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var batch = new[]
            {
                Restriction(
                    5,
                    id: RestrictionId(5)
                ),

                Restriction(
                    7,
                    id: RestrictionId(7)
                )
            };

            session.AddRestrictions(
                batch,
                T0
            );

            Assert.Equal(
                22,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                3,
                session.ActiveRestrictionIds.Count
            );

            Assert.Contains(
                RestrictionId(5),
                session.ActiveRestrictionIds
            );

            Assert.Contains(
                RestrictionId(7),
                session.ActiveRestrictionIds
            );

            Assert.Contains(
                RestrictionId(10),
                session.ActiveRestrictionIds
            );
        }

        [Fact]
        public void AddRestrictions_ExtendsExpirationFromOriginalCreationTime()
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            session.AddRestrictions(
                new[] {
                    Restriction(
                        20,
                        id: RestrictionId(20)
                    )
                },
                T0.AddMinutes(5)
            );

            Assert.Equal(
                T0.AddMinutes(30),
                session.GetExpirationDate()
            );
        }

        [Fact]
        public void AddRestrictions_ExpiredSession_UsesUpdatedTotalDuration()
        {
            var session = RestoreSession(
                new[] { RestrictionId(10) },
                10
            );

            var now = T0.AddMinutes(15);

            Assert.True(session.IsExpiredAt(now));

            session.AddRestrictions(
                new[] { Restriction(
                    20,
                    id: RestrictionId(20))
                },
                now
            );

            Assert.Equal(
                30,
                session.TotalBlockedMinutes
            );

            Assert.True(session.IsActive(now));
        }

        [Fact]
        public void AddRestrictions_AfterRemovingAllRestrictions_PopulatesSession()
        {
            var items = DefaultThree();
            var session = CreateSession(items);

            session.RemoveRestrictions(
                items,
                T0
            );

            Assert.Empty(session.ActiveRestrictionIds);

            Assert.Equal(
                0,
                session.TotalBlockedMinutes
            );

            session.AddRestrictions(
                new[] { Restriction(
                    15,
                    id: RestrictionId(15))
                },
                T0
            );

            Assert.Single(session.ActiveRestrictionIds);

            Assert.Equal(
                15,
                session.TotalBlockedMinutes
            );
        }

        [Fact]
        public void AddRestrictions_AfterValidation_RevalidatesInput()
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var item = Restriction(
                5,
                id: RestrictionId(5)
            );

            session.ValidateCanAddRestrictions(
                new[] { item },
                T0
            );

            var completed = Restriction(
                5,
                status: RestrictionStatus.Completed,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    session.AddRestrictions(
                        new[] { completed },
                        T0
                    )
                );

                Assert.Contains(
                    "Only active restrictions",
                    exception.Context["Rule"]!.ToString()
                );
            });
        }
        #endregion

        #region Removing restrictions: validation
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrRemove_DifferentOwner_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var foreign = Restriction(
                10,
                User("222222222222"),
                id: RestrictionId(10)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            new[] { foreign },
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            new[] { foreign },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "A restriction belonging to another user cannot be removed from this restriction session.",
                    "Update"
                );

                Assert.Same(
                    session.UserId,
                    exception.Context["State_CurrentUserId"]
                );

                Assert.Same(
                    foreign.UserId,
                    exception.Context["State_TargetUserId"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrRemove_DifferentRestrictionType_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var other = Restriction(
                10,
                type: RestrictionType.ChatMute,
                id: RestrictionId(10)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            new[] { other },
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            new[] { other },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "A restriction with a different type cannot be removed from this restriction session.",
                    "Update"
                );

                Assert.Equal(
                    RestrictionType.Ban,
                    exception.Context["State_CurrentRestrictionType"]
                );

                Assert.Equal(
                    RestrictionType.ChatMute,
                    exception.Context["State_TargetRestrictionType"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrRemove_MissingId_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(Restriction(
                10,
                id: RestrictionId(10))
            );

            var missing = Restriction(
                5,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            new[] { missing },
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            new[] { missing },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "The restriction session does not contain the restriction being removed.",
                    "Update"
                );

                Assert.Equal(
                    session.Id.Value,
                    exception.Context["State_SessionId"]
                );

                Assert.Equal(
                    missing.Id.Value,
                    exception.Context["State_RestrictionId"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrRemove_DuplicateIncomingIds_ThrowsInvariant(bool validateOnly)
        {
            var session = CreateSession(
                Restriction(
                    10,
                    id: RestrictionId(10)
                ),

                Restriction(
                    20,
                    id: RestrictionId(20)
                )
            );
            var a = Restriction(
                10,
                id: RestrictionId(10)
            );

            var b = Restriction(
                10,
                id: RestrictionId(10)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            new[] { a, b },
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            new[] { a, b },
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "Restrictions being removed contain duplicate identifiers.",
                    "Update"
                );

                Assert.Equal(
                    session.Id.Value,
                    exception.Context["State_SessionId"]
                );

                Assert.Equal(
                    a.Id.Value,
                    exception.Context["State_DuplicateRestrictionId"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrRemove_DurationSumOverflow_ThrowsOverflowException(bool validateOnly)
        {
            var session = RestoreSession(
                new[] {
                    RestrictionId(1),
                    RestrictionId(2)
                },
                int.MaxValue
            );

            var items = new[]
            {
                Restriction(
                    int.MaxValue,
                    id: RestrictionId(1)
                ),

                Restriction(
                    1,
                    id: RestrictionId(2)
                )
            };

            CaptureAndAssertUnchanged(session, () =>
            {
                Assert.Throws<OverflowException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            items,
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            items,
                            T0
                        );
                    }
                });
            });
        }

        [Theory]
        [InlineData(true, true, 0)]
        [InlineData(true, true, -1)]
        [InlineData(true, false, 5)]
        [InlineData(true, false, -1)]
        [InlineData(false, true, 0)]
        [InlineData(false, true, -1)]
        [InlineData(false, false, 5)]
        [InlineData(false, false, -1)]
        public void ValidateOrRemove_InconsistentResult_ThrowsInvariant(
            bool validateOnly,
            bool leaveIds,
            int resultingMinutes)
        {
            const int sessionMinutes = 10;

            var removeMinutes = sessionMinutes - resultingMinutes;

            var ids = leaveIds
                ? new[] {
                    RestrictionId(10),
                    RestrictionId(20)
                }
                : new[] { RestrictionId(20) };

            var session = RestoreSession(ids, sessionMinutes);

            var toRemove = new[]
            {
                Restriction(
                    removeMinutes,
                    id: RestrictionId(20)
                )
            };

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            toRemove,
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            toRemove,
                            T0
                        );
                    }
                });

                AssertInvariant(
                    exception,
                    "Removing the restrictions would produce an inconsistent restriction session.",
                    "Update"
                );

                Assert.Equal(
                    10,
                    exception.Context["State_CurrentTotalBlockedMinutes"]
                );

                Assert.Equal(
                    resultingMinutes,
                    exception.Context["State_UpdatedTotalBlockedMinutes"]
                );

                Assert.Equal(
                    leaveIds ? 1 : 0,
                    exception.Context["State_RemainingRestrictionCount"]
                );
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateOrRemove_ValidationOrder_ReportsFirstViolation(bool validateOnly)
        {
            var session = CreateSession(Restriction(10, id: RestrictionId(10)));

            var foreignOtherType = Restriction(
                10,
                User("222222222222"),
                RestrictionType.ChatMute,
                id: RestrictionId(10)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var ownerFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            new[] { foreignOtherType },
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            new[] { foreignOtherType },
                            T0
                        );
                    }
                });

                Assert.Contains(
                    "belonging to another user",
                    ownerFirst.Context["Rule"]!.ToString()
                );
            });

            var missingOtherType = Restriction(
                5,
                type: RestrictionType.ChatMute,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var typeFirst = Assert.Throws<DomainInvariantViolationException>(() =>
                {
                    if (validateOnly)
                    {
                        session.ValidateCanRemoveRestrictions(
                            new[] { missingOtherType },
                            T0
                        );
                    }
                    else
                    {
                        session.RemoveRestrictions(
                            new[] { missingOtherType },
                            T0
                        );
                    }
                });

                Assert.Contains(
                    "different type",
                    typeFirst.Context["Rule"]!.ToString()
                );
            });
        }
        #endregion

        #region ValidateCanRemoveRestrictions
        [Theory]
        [InlineData(30, 0, (int)RestrictionSessionRemovalEffect.RemainsActive)]
        [InlineData(30, 29, (int)RestrictionSessionRemovalEffect.RemainsActive)]
        [InlineData(30, 30, (int)RestrictionSessionRemovalEffect.BecomesExpired)]
        [InlineData(30, 45, (int)RestrictionSessionRemovalEffect.BecomesExpired)]
        [InlineData(60, 0, (int)RestrictionSessionRemovalEffect.BecomesEmpty)]
        [InlineData(30, 60, (int)RestrictionSessionRemovalEffect.AlreadyExpired)]
        [InlineData(60, 60, (int)RestrictionSessionRemovalEffect.AlreadyExpired)]
        public void ValidateCanRemoveRestrictions_ReturnsExpectedRemovalEffectAndPreservesState(
            int minutesToRemove,
            int minutesFromCreated,
            int expectedEffect)
        {
            var expected = (RestrictionSessionRemovalEffect)expectedEffect;
            var items = DefaultThree();
            var session = CreateSession(items);

            var toRemove = minutesToRemove == 30
                ? new[] { items.Single(r => r.RestrictionRule.DurationMinutes == 30) }
                : items;

            var now = T0.AddMinutes(minutesFromCreated);

            CaptureAndAssertUnchanged(session, () =>
            {
                var effect = session.ValidateCanRemoveRestrictions(
                    toRemove,
                    now
                );

                Assert.Equal(
                    expected,
                    effect
                );
            });
        }
        #endregion

        #region RemoveRestrictions
        [Fact]
        public void RemoveRestrictions_PartialBatch_RemovesIdsAndSubtractsDuration()
        {
            var items = DefaultThree();
            var session = CreateSession(items);
            var remove = new[] { items.Single(r => r.RestrictionRule.DurationMinutes == 30) };

            session.RemoveRestrictions(
                remove,
                T0
            );

            Assert.Equal(
                30,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                2,
                session.ActiveRestrictionIds.Count
            );

            Assert.True(
                session.ActiveRestrictionIds.ToHashSet().SetEquals(
                    new[] {
                        RestrictionId(10),
                        RestrictionId(20)
                    }
                )
            );

            Assert.Equal(
                T0.AddMinutes(30),
                session.GetExpirationDate()
            );
        }

        [Fact]
        public void RemoveRestrictions_AllIds_ProducesEmptySession()
        {
            var items = DefaultThree();
            var session = CreateSession(items);
            var expectedId = session.Id;
            var expectedUserId = session.UserId;
            var expectedType = session.RestrictionType;
            var expectedCreatedAt = session.CreatedAt;

            session.RemoveRestrictions(items, T0);

            Assert.Empty(session.ActiveRestrictionIds);

            Assert.Equal(
                0,
                session.TotalBlockedMinutes
            );

            Assert.Same(
                expectedId,
                session.Id
            );

            Assert.Same(
                expectedUserId,
                session.UserId
            );

            Assert.Equal(
                expectedType,
                session.RestrictionType
            );

            Assert.Equal(
                expectedCreatedAt,
                session.CreatedAt
            );
        }

        [Theory]
        [InlineData(RestrictionStatus.Active)]
        [InlineData(RestrictionStatus.Completed)]
        [InlineData(RestrictionStatus.Revoked)]
        public void RemoveRestrictions_DifferentRestrictionStatuses_AreAcceptedAndPreserved(RestrictionStatus status)
        {
            var active = Restriction(
                10,
                id: RestrictionId(10)
            );

            var session = CreateSession(active);

            var toRemove = Restriction(
                10,
                status: status,
                id: RestrictionId(10)
            );

            session.RemoveRestrictions(
                new[] { toRemove },
                T0
            );

            Assert.Empty(session.ActiveRestrictionIds);

            Assert.Equal(
                0,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                status,
                toRemove.Status
            );
        }

        [Fact]
        public void RemoveRestrictions_ExpiredSession_StillAppliesRemoval()
        {
            var items = DefaultThree();
            var session = CreateSession(items);
            var now = T0.AddMinutes(90);

            Assert.True(session.IsExpiredAt(now));

            session.RemoveRestrictions(
                new[] { items.Single(r => r.RestrictionRule.DurationMinutes == 30) },
                now);

            Assert.Equal(
                30,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                2,
                session.ActiveRestrictionIds.Count
            );
        }

        [Fact]
        public void RemoveRestrictions_RepeatedRemoval_ThrowsInvariantAndPreservesState()
        {
            var items = DefaultThree();
            var session = CreateSession(items);
            var target = items.Single(r => r.RestrictionRule.DurationMinutes == 30);

            session.RemoveRestrictions(
                new[] { target },
                T0
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    session.RemoveRestrictions(
                        new[] { target },
                        T0
                    )
                );

                Assert.Contains(
                    "does not contain the restriction",
                    exception.Context["Rule"]!.ToString()
                );
            });
        }

        [Fact]
        public void RemoveRestrictions_AfterValidation_RevalidatesInput()
        {
            var items = DefaultThree();
            var session = CreateSession(items);
            var target = items.Single(r => r.RestrictionRule.DurationMinutes == 30);

            session.ValidateCanRemoveRestrictions(
                new[] { target },
                T0
            );

            var unknown = Restriction(
                5,
                id: RestrictionId(5)
            );

            CaptureAndAssertUnchanged(session, () =>
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    session.RemoveRestrictions(
                        new[] { target, unknown },
                        T0
                    )
                );

                Assert.Contains(
                    "does not contain the restriction",
                    exception.Context["Rule"]!.ToString()
                );
            });
        }
        #endregion

        #region Queries after removing all restrictions
        [Fact]
        public void Queries_AfterRemovingAllRestrictions_ReturnEmptySessionResults()
        {
            var items = DefaultThree();
            var session = CreateSession(items);

            session.RemoveRestrictions(
                items,
                T0
            );

            Assert.Equal(
                T0,
                session.GetExpirationDate()
            );

            Assert.False(session.IsActive(T0));

            Assert.Equal(
                TimeSpan.Zero,
                session.GetRemainingTime(T0)
            );

            Assert.True(session.IsExpiredAt(T0));
        }
        #endregion
    }
}
