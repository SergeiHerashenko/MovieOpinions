using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Validation
{
    public class ValidationFailure
    {
        public required Error Error { get; init; }
    }
}
