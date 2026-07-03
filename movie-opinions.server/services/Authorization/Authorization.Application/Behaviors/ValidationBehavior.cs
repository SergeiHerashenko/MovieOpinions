using Authorization.Application.Common.Exceptions;
using Authorization.Domain.Common.Errors;
using Authorization.Domain.Results;
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

            if(responseType == typeof(Result))
                return (TResponse)(object)Result.Failure(errors);

            if(responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = responseType.GetGenericArguments()[0];

                var failureMethod = typeof(Result<>)
                    .MakeGenericType(dataType)
                    .GetMethod(nameof(Result.Failure), new[] { typeof(IReadOnlyList<Error>) });

                if (failureMethod is null)
                    throw ApplicationInvalidOperationException.ValueAccessOnFailure<TResponse>(nameof(failureMethod));

                return (TResponse)failureMethod.Invoke(null, new object[] { errors })!;
            }

            throw ApplicationInvalidOperationException.ValueAccessOnFailure<TResponse>(responseType.Name);
        }
    }
}
