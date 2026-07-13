using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<
    TRequest,
    TResponse>
    : IPipelineBehavior<
        TRequest,
        TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<
            IValidator<TRequest>> _validators;

        public ValidationBehavior(
            IEnumerable<
                IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next(
                    cancellationToken);
            }

            var validationContext =
                new ValidationContext<TRequest>(
                    request);

            var validationResults =
                await Task.WhenAll(
                    _validators.Select(
                        validator =>
                            validator
                                .ValidateAsync(
                                    validationContext,
                                    cancellationToken)));

            var validationFailures =
                validationResults
                    .SelectMany(
                        result =>
                            result.Errors)
                    .Where(
                        failure =>
                            failure is not null)
                    .ToArray();

            if (validationFailures.Length > 0)
            {
                throw new ValidationException(
                    validationFailures);
            }

            return await next(
                cancellationToken);
        }
    }
}
