using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Shared.Services.Commons;

namespace API.Behaviors;

public class FailFastRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> where TResponse : Response, new()
{
    private readonly IEnumerable<IValidator> _validators;
    private readonly ILogger<TResponse> _logger;

    public FailFastRequestBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<TResponse> logger)
    {
        _logger = logger;
        _validators = validators;
    }
    private Task<TResponse> Errors(IEnumerable<ValidationFailure> failures)
    {
        var response = new TResponse();

        foreach (var failure in failures)
        {
            response.AddError(failure.ErrorMessage);

            _logger.LogError($"Error ao validar dados: Propriedade: {failure.PropertyName} -- Erro: {failure.ErrorMessage} -- Type: { failure.ErrorCode} ");

        }

        return Task.FromResult(response as TResponse);
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Entering LoggingBehavior with request {Name}", typeof(TRequest).Name);           

        var failures = _validators
            .Select(v => v.Validate((IValidationContext)request))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        return failures.Any()
            ? Errors(failures)
            : next();
    }
}