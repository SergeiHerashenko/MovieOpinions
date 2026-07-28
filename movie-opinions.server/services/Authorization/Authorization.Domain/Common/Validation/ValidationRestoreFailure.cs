namespace Authorization.Domain.Common.Validation
{
    public sealed class ValidationRestoreFailure : ValidationFailure
    {
        public required Func<Exception> BuildException { get; init; }
    }
}
