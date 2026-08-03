using ErrorOr;
using FeedInsight.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                // 1. Check if TResponse is an ErrorOr type
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
                {
                    // 2. Map FluentValidation errors to ErrorOr errors
                    var errors = failures.ConvertAll(f => Error.Validation(f.PropertyName, f.ErrorMessage));

                    // 3. Create an ErrorOr instance using reflection
                    return (TResponse)typeof(ErrorOr<>)
                        .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                        .GetMethod("From", new[] { typeof(List<Error>) })!
                        .Invoke(null, new object[] { errors })!;
                }

                // Fallback for non-ErrorOr requests (keep throwing if not using ErrorOr)
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
