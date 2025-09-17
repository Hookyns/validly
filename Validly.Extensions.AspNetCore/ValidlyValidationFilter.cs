using Microsoft.AspNetCore.Http;

namespace Validly.Extensions.AspNetCore;

/// <summary>
/// Endpoint filter executing validations for parameters
/// </summary>
public class ValidlyValidationFilter : IEndpointFilter
{
	private readonly IServiceProvider _serviceProvider;

	/// <param name="serviceProvider"></param>
	public ValidlyValidationFilter(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	/// <inheritdoc />
	public async ValueTask<object?> InvokeAsync(
		EndpointFilterInvocationContext invocationContext,
		EndpointFilterDelegate next
	)
	{
		var ct = invocationContext.HttpContext.RequestAborted;
		for (int argumentIndex = 0; argumentIndex < invocationContext.Arguments.Count; argumentIndex++)
		{
			object? argument = invocationContext.Arguments[argumentIndex];

			if (argument is IValidatable validatable)
			{
				// Do not use "using"; we don't want to dispose it here.
				// The ValidationHttpResult will dispose in case there are errors.
				var resultTask = validatable.ValidateAsync(_serviceProvider, ct);

				ValidationResult validationResult;

				if (resultTask.IsCompletedSuccessfully)
				{
					validationResult = resultTask.Result;
				}
				else
				{
					validationResult = await resultTask;
				}

				try
				{
					if (!validationResult.IsSuccess)
					{
						// TODO: This allocates memory. Optimize! For disposing and returning back to the pool we can use invocationContext.HttpContext.Response.RegisterForDispose(disposable)
						return new ValidationHttpResult(validationResult);
					}

					// Dispose if not used for ValidationHttpResult
					validationResult.Dispose();
				}
				catch (Exception)
				{
					// Try to Dispose
					validationResult.Dispose();
				}
			}
		}

		return await next(invocationContext);
	}
}
