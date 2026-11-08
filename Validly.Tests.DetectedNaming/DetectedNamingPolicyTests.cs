using System.Text.Json;
using Validly.Extensions.Validators.Strings;

namespace Validly.Tests.DetectedNaming;

/// <summary>
/// The serializer setup of the "application"; the generator finds this policy and uses it at compile time
/// </summary>
public static class SerializerSetup
{
	public static JsonSerializerOptions CreateOptions()
	{
		return new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower };
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
	public void DetectedPolicy_MatchesTheConfiguredSerializer()
	{
		Assert.Equal("user-name", JsonNamingPolicy.KebabCaseLower.ConvertName(nameof(KebabCaseRequest.UserName)));
		Assert.Same(JsonNamingPolicy.KebabCaseLower, SerializerSetup.CreateOptions().PropertyNamingPolicy);
	}
}
