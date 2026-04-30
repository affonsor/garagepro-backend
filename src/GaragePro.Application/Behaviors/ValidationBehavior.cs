using System.Reflection;
using FluentValidation;
using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var errors = failures.Select(f => f.ErrorMessage).ToList();

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var method = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(Result<object>.ValidationFailure),
                    BindingFlags.Public | BindingFlags.Static,
                    [typeof(IEnumerable<string>)]);
            return (TResponse)method!.Invoke(null, [errors])!;
        }

        throw new ValidationException(failures);
    }
}
