using System.Text.Json.Serialization;
using Validly.Extensions.Validators.Strings;
using Validly.Naming;

namespace Validly.Tests.CompileTimeNaming;

[Validatable]
partial class SnakeCaseRequest
{
	[MinLength(5)]
	public required string UserName { get; init; }

	[MinLength(5)]
	[JsonPropertyName("explicitName")]
	public required string Explicit { get; init; }

	[MinLength(5)]
	public required string XMLReader { get; init; }
}

[Validatable]
partial class SnakeCaseRoot
{
	public required SnakeCaseChild ChildObject { get; init; }
}

[Validatable]
partial class SnakeCaseChild
{
	[MinLength(5)]
	public required string ChildProp { get; init; }
}

/// <summary>
/// The project configures &lt;JsonNamingPolicy&gt;snake_case_lower&lt;/JsonNamingPolicy&gt;, so the generator bakes the
/// JSON names into the generated code and nothing has to be converted at runtime.
/// </summary>
public class ConfiguredNamingPolicyTests
{
	private static readonly SnakeCaseRequest Invalid = new()
	{
		UserName = "abc",
		Explicit = "abc",
		XMLReader = "abc",
	};

	[Fact]
	public async Task ConfiguredPolicy_IsAppliedWithoutRuntimePolicy()
	{
		using var result = await Invalid.ValidateAsync();

		var pointers = result.GetProblemDetails().Errors.Select(x => x.Pointer).ToArray();

		Assert.Contains("#/user_name", pointers);
		Assert.Contains("#/xml_reader", pointers);

		// [JsonPropertyName] wins and the policy is not applied to it
		Assert.Contains("#/explicitName", pointers);
	}

	[Fact]
	public async Task CompileTimeNames_AreNotOverriddenByRuntimePolicy()
	{
		using var result = await Invalid.ValidateAsync();

		var pointers = result.GetProblemDetails(PropertyNamingPolicy.KebabCaseUpper).Errors.Select(x => x.Pointer);

		Assert.Contains("#/user_name", pointers);
	}

	[Fact]
	public async Task NestedProperties_UseConfiguredPolicyForEverySegment()
	{
		var root = new SnakeCaseRoot { ChildObject = new() { ChildProp = "abc" } };

		using var result = await root.ValidateAsync();

		Assert.Equal("#/child_object/child_prop", result.GetProblemDetails().Errors.Single().Pointer);
	}
}
