namespace Authorization.Application.DomainEvents.UserRestrictionSessions.Data
{
    public sealed class SessionRestrictionNotificationData
    {
        public IReadOnlyCollection<RestrictionInfo> RestrictionDetails { get; }

        public string RestrictionType { get; }

        public int TotalBlockedMinutes { get; }

        public SessionRestrictionNotificationData(
            IReadOnlyCollection<RestrictionInfo> restrictionDetails,
            int totalBlockedMinutes,
            string restrictionType)
        {
            RestrictionDetails = restrictionDetails;
            TotalBlockedMinutes = totalBlockedMinutes;
            RestrictionType = restrictionType;
        }
    }

    public class RestrictionInfo
    {
        public string NameRestriction { get; }

        public int RestrictionMinutes { get; }

        public RestrictionInfo(string nameRestriction, int restrictionMinutes)
        {
            NameRestriction = nameRestriction;
            RestrictionMinutes = restrictionMinutes;
        }
    }

    public sealed class RestrictionDetails : RestrictionInfo
    {
        public string ReasonRestriction { get; }

        public RestrictionDetails(string nameRestriction, int restrictionMinutes, string? reasonRestriction)
            : base(nameRestriction, restrictionMinutes)
        {
            ReasonRestriction = reasonRestriction ?? string.Empty;
        }
    }
}
