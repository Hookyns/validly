using System.Text;
using Microsoft.AspNetCore.Http;

namespace Validly.Extensions.AspNetCore;

/// <summary>
/// Endpoint filter executing validations for parameters
/// </summary>
public sealed class ValidlyValidationFilter : IEndpointFilter
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
						return TypedResults.Content(
							validationResult.GetProblemDetailsJson(),
							"application/problem+json",
							Encoding.UTF8,
							StatusCodes.Status400BadRequest
						);
					}
				}
				finally
				{
					// The problem details JSON is already materialized at this point,
					// so it's safe to return the result back to the pool here.
					validationResult.Dispose();
				}
			}
		}

		return await next(invocationContext);
	}
}
