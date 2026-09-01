using Authorization.Domain.Common.Errors;
using FluentValidation;

namespace Authorization.Application.Common.Validation
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithDomainError<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            Error error)
        {
            return rule
                .WithErrorCode(error.Code)
                .WithMessage(error.Message);
        }

        public static IRuleBuilderOptions<T, TProperty> WithApplicationError<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            Error error)
        {
            return rule
                .WithErrorCode(error.Code)
                .WithMessage(error.Message);
        }
    }
}
