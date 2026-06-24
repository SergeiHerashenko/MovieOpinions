using Authorization.Application.Common.Exceptions;
using Authorization.Application.Result;
using Authorization.Domain.Common.Errors;
using FluentValidation;
using MediatR;

namespace Authorization.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResult = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResult
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if(failures.Count == 0)
                return await next();

            // Перетворюємо помилки FluentValidation на доменні Error
            var errors = failures
                .Select(f => new Error(
                    f.ErrorCode, 
                    f.ErrorMessage, 
                    ErrorType.Validation))
                .ToList();

            return CreateFailureResult(errors);
        }

        private static TResponse CreateFailureResult(List<Error> errors)
        {
            var responseType = typeof(TResponse);

            if(responseType == typeof(ApplicationResult))
                return (TResponse)(object)ApplicationResult.Failure(errors);

            if(responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(ApplicationResult<>))
            {
                var dataType = responseType.GetGenericArguments()[0];

                var failureMethod = typeof(ApplicationResult<>)
                    .MakeGenericType(dataType)
                    .GetMethod(nameof(ApplicationResult.Failure), new[] { typeof(IReadOnlyList<Error>) });

                if (failureMethod is null)
                    throw ApplicationInvalidOperationException.ValueAccessOnFailure(
                        $"ApplicationResult<{dataType.Name}> does not have a Failure(IEnumerable<Error>) method. " +
                        "ValidationBehavior cannot construct a failure response.",
                        new Dictionary<string, object>
                        {
                            ["layer"] = "Application.Behaviors",
                            ["behavior"] = nameof(ValidationBehavior<TRequest, TResponse>),
                            ["response_type"] = dataType.Name,
                            ["expected_method"] = "ApplicationResult<T>.Failure(IEnumerable<Error>)",
                            ["issue"] = "Reflection failed to locate Failure factory method",
                            ["possible_causes"] = new[]
                            {
                                "Method signature changed",
                                "Method removed or renamed",
                                "Generic type mismatch",
                                "Incorrect binding flags or overload resolution failure"
                            }
                        }
                    );

                return (TResponse)failureMethod.Invoke(null, new object[] { errors })!;
            }

            throw ApplicationInvalidOperationException.ValueAccessOnFailure(
                $"ValidationBehavior requires TResponse to be ApplicationResult or ApplicationResult<T>, " +
                $"but got '{responseType.Name}'. This violates the project rule that all handlers return ApplicationResult.",
                new Dictionary<string, object>
                {
                    ["layer"] = "Application.Behaviors",
                    ["behavior"] = nameof(ValidationBehavior<TRequest, TResponse>),
                    ["response_type"] = responseType.Name,
                    ["expected"] = "ApplicationResult or ApplicationResult<T>",
                    ["rule"] = "All MediatR handlers must return ApplicationResult types",
                    ["issue"] = "Non-result type detected in pipeline",
                    ["fix"] = "Ensure IRequest<TResponse> uses ApplicationResult or ApplicationResult<T>"
                }
            );
        }
    }
}
