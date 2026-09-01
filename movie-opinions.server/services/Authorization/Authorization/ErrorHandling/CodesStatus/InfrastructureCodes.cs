using Authorization.Infrastructure.Errors;

namespace Authorization.ErrorHandling.CodesStatus
{
    internal static class InfrastructureCodes
    {
        public static Dictionary<string, int> Values { get; } =
            new Dictionary<string, int>
            {
                [InfrastructureErrorCodes.IntegrationError.SendingError] = StatusCodes.Status502BadGateway,

                [InfrastructureErrorCodes.ConfigurationError.NotFoundValue] = StatusCodes.Status500InternalServerError,

                [InfrastructureErrorCodes.LimitError.MaxLimit] = StatusCodes.Status429TooManyRequests,

                [InfrastructureErrorCodes.DbError.ConnectionStringNotFound] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.NoTransaction] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.NotFound] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.MigrationError] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.DatabaseError] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.NestedTransaction] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.DataConsistency] = StatusCodes.Status500InternalServerError,
                [InfrastructureErrorCodes.DbError.NotConsistentState] = StatusCodes.Status500InternalServerError,

                [InfrastructureErrorCodes.UsereContextError.InvalidUserContext] = StatusCodes.Status401Unauthorized,
            };
    }
}
