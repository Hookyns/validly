using Microsoft.AspNetCore.Http;

namespace Validly.Extensions.AspNetCore;

/// <summary>
/// HTTP result for Validly ValidationResult
/// </summary>
public sealed class ValidationHttpResult : IResult
{
	private readonly ValidationResult _validationResult;

	/// <param name="validationResult"></param>
	public ValidationHttpResult(ValidationResult validationResult)
	{
		_validationResult = validationResult;
	}

	/// <inheritdoc />
	public async Task ExecuteAsync(HttpContext httpContext)
	{
		httpContext.Response.ContentType = "application/problem+json";
		httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

		try
		{
			await httpContext.Response.WriteAsync(_validationResult.GetProblemDetailsJson());
		}
		finally
		{
			_validationResult.Dispose();
		}
	}
}
