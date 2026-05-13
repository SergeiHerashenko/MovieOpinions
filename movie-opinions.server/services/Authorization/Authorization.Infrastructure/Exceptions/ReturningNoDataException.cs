namespace Authorization.Infrastructure.Exceptions
{
    public sealed class ReturningNoDataException : BaseInfrastructureException
    {
        private const string DefaultErrorCode = "DATABASE_RECORD_NOT_FOUND";

        public ReturningNoDataException(string entityName, object identifier)
            : base(DefaultErrorCode, $"Не вдалося отримати дані для сутності '{entityName}' за ідентифікатором: {identifier}") { }

        public ReturningNoDataException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
