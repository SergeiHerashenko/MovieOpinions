using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        public Result AddRestrictions(IEnumerable<RestrictionData> restrictionDataCollection, DateTimeOffset now)
        {
            if (restrictionDataCollection is null)
                return Result.Failure(RestrictionErrors.EmptyRestrictionList<User>());

            var restrictionData = restrictionDataCollection.ToList();

            if (restrictionData.Count == 0)
                return Result.Failure(RestrictionErrors.EmptyRestrictionList<User>());

            var createRestrictionsResult = CreateRestrictions(restrictionData);

            if (createRestrictionsResult.IsFailure)
                return createRestrictionsResult;

            foreach (var restrictionGroup in createRestrictionsResult.Value.GroupBy(x => x.RestrictionType))
            {
                var session = _restrictionSessions
                    .FirstOrDefault(x => x.RestrictionType == restrictionGroup.Key);

                var restrictionDescription = restrictionDataCollection
                    .Select(x => (Rule: x.RestrictionRule, x.Reason))
                    .ToArray();

                if (session is null)
                {
                    var createSessionResult = UserRestrictionSession.Create(Id, restrictionGroup);

                    if (createSessionResult.IsFailure)
                        return createSessionResult;

                    _restrictionSessions.Add(createSessionResult.Value);

                    AddDomainEvent(new UserRestrictionSessionCreatedEvent(
                        createSessionResult.Value.Id,
                        Login,
                        restrictionDescription,
                        createSessionResult.Value.RestrictionType,
                        createSessionResult.Value.TotalBlockedMinutes,
                        now)
                    );

                    AddAggregateChange(new UserRestrictionSessionCreated(createSessionResult.Value, now));

                    continue;
                }

                var addRestrictionsResult = session.AddRestrictions(restrictionGroup);

                if (addRestrictionsResult.IsFailure)
                    return addRestrictionsResult;

                AddDomainEvent(new UserRestrictionSessionAddRestrictionEvent(
                    session.Id,
                    Login,
                    restrictionDescription,
                    session.RestrictionType,
                    session.TotalBlockedMinutes,
                    now)
                );

                AddAggregateChange(new UserRestrictionSessionUpdated(session, now));
            }

            _restrictions.AddRange(createRestrictionsResult.Value);

            AddRestrictionCreatedChange(createRestrictionsResult.Value, now);

            return Result.Success();
        }

        public Result RemoveRestriction(UserRestrictionId userRestrictionId, DateTimeOffset now)
        {
            if (userRestrictionId is null)
                return Result.Failure(CommonErrors.Identifier.EmptyIdentifier<UserRestrictionId>(nameof(userRestrictionId)));

            var restriction = _restrictions
                .FirstOrDefault(x => x.Id == userRestrictionId);

            if (restriction is null)
                return Result.Failure(RestrictionErrors.NotFoundRestriction<User>());

            var resultCancel = restriction.CancelRestriction(now);

            if (resultCancel.IsFailure)
                return resultCancel;

            var session = _restrictionSessions
                .FirstOrDefault(x => x.RestrictionType == restriction.RestrictionType);

            if (session is null)
                return Result.Failure(RestrictionErrors.NotFoundSession<User>(restriction.RestrictionType.ToString()));

            var removeResult = session.RemoveRestriction(restriction);

            if (removeResult.IsFailure)
                return removeResult;

            if (session.IsEmpty())
            {
                _restrictionSessions.Remove(session);

                AddAggregateChange(new UserRestrictionSessionDeleted(session.Id, now));
            }
            else
            {
                AddAggregateChange(new UserRestrictionSessionUpdated(session, now));
            }

            _restrictions.Remove(restriction);

            AddDomainEvent(new UserRestrictionSessionRemovedRestrictionEvent(
                session.Id,
                Login,
                restriction.RestrictionRule,
                restriction.RestrictionType,
                session.TotalBlockedMinutes,
                now)
            );

            AddAggregateChange(new UserRestrictionUpdated(restriction, now));

            return Result.Success();
        }

        public Result RemoveRestrictionSession(RestrictionType restrictionType, DateTimeOffset now)
        {
            var session = _restrictionSessions
                .FirstOrDefault(x => x.RestrictionType == restrictionType);

            if (session is null)
                return Result.Failure(RestrictionErrors.NotFoundSessionType<User>(restrictionType.ToString()));

            var restrictions = _restrictions
                .Where(x =>
                    !x.IsRevoked &&
                    x.RestrictionType == restrictionType)
                .ToList();

            foreach (var restriction in restrictions)
            {
                if (!session.ContainsRestriction(restriction.Id))
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Restriction session is inconsistent with active restrictions!",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id,
                            ["RestrictionId"] = restriction.Id
                        });
                }
            }

            foreach (var restriction in restrictions)
            {
                var result = restriction.CancelRestriction(now);

                if (result.IsFailure)
                    return result;

                _restrictions.Remove(restriction);

                AddAggregateChange(new UserRestrictionUpdated(restriction, now));
            }

            _restrictionSessions.Remove(session);

            AddDomainEvent(new UserRestrictionSessionRemovedEvent(
                session.Id,
                Login,
                session.RestrictionType,
                now)
            );

            AddAggregateChange(new UserRestrictionSessionDeleted(session.Id, now));

            return Result.Success();
        }

        private Result<List<UserRestriction>> CreateRestrictions(IEnumerable<RestrictionData> restrictionDataCollection)
        {
            var restrictions = new List<UserRestriction>();

            foreach (var data in restrictionDataCollection)
            {
                var result = UserRestriction.Create(
                    Id,
                    data.RestrictionType,
                    data.RestrictionRule,
                    data.RestrictedBy,
                    data.Reason);

                if (result.IsFailure)
                    return Result<List<UserRestriction>>.Failure(result.Errors);

                restrictions.Add(result.Value);
            }

            return Result<List<UserRestriction>>.Success(restrictions);
        }

        private void AddRestrictionCreatedChange(IEnumerable<UserRestriction> userRestrictions, DateTimeOffset now)
        {
            foreach (var restriction in userRestrictions)
            {
                AddAggregateChange(new UserRestrictionCreated(restriction, now));
            }
        }
    }
}
