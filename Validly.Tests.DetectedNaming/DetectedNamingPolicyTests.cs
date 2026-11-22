using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Validly.Extensions.AspNetCore;
using Validly.Extensions.Validators.Strings;

namespace Validly.Tests.DetectedNaming;

/// <summary>
/// The JSON setup of the "application"; the generator finds the policy of the HTTP JSON options and uses it at compile time
/// </summary>
public static class SerializerSetup
{
	public static IServiceCollection AddJson(this IServiceCollection services)
	{
		return services.ConfigureHttpJsonOptions(options =>
			options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
		);
	}

	/// <summary>
	/// Serializer of some other API; it has nothing to do with the HTTP JSON options, so the generator has to ignore it
	/// </summary>
	public static JsonSerializerOptions CreateClientOptions()
	{
		return new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper };
	}
}

[Validatable]
partial class KebabCaseRequest
{
	[MinLength(5)]
	public required string UserName { get; init; }
}

public class DetectedNamingPolicyTests
{
	[Fact]
	public async Task DetectedPolicy_IsAppliedWithoutRuntimePolicy()
	{
		var request = new KebabCaseRequest { UserName = "abc" };

		using var result = await request.ValidateAsync();

		Assert.Equal("#/user-name", result.GetProblemDetails().Errors.Single().Pointer);
	}

	[Fact]
	public void DetectedPolicy_MatchesTheConfiguredHttpJsonOptions()
	{
		using ServiceProvider services = new ServiceCollection().AddJson().BuildServiceProvider();
		JsonNamingPolicy? configured = services
			.GetRequiredService<IOptions<JsonOptions>>()
			.Value.SerializerOptions.PropertyNamingPolicy;

		Assert.Same(JsonNamingPolicy.KebabCaseLower, configured);
		Assert.Equal("user-name", configured!.ConvertName(nameof(KebabCaseRequest.UserName)));
	}

	[Fact]
	public async Task Filter_ReturnsProblemJsonWithPointersOfTheHttpJsonOptions()
	{
		using ServiceProvider services = new ServiceCollection().AddLogging().AddJson().BuildServiceProvider();
		var httpContext = new DefaultHttpContext { RequestServices = services };
		var filter = new ValidlyValidationFilter(services);

		object? result = await filter.InvokeAsync(
			new DefaultEndpointFilterInvocationContext(httpContext, new KebabCaseRequest { UserName = "abc" }),
			_ => throw new InvalidOperationException("The endpoint must not be invoked for an invalid request")
		);

		var content = Assert.IsType<ContentHttpResult>(result);
		Assert.Equal(StatusCodes.Status400BadRequest, content.StatusCode);
		Assert.Equal("application/problem+json", content.ContentType);
		Assert.Contains("\"pointer\": \"#/user-name\"", content.ResponseContent);

		httpContext.Response.Body = new MemoryStream();
		await content.ExecuteAsync(httpContext);
		Assert.Equal("application/problem+json", httpContext.Response.ContentType);
	}
}
