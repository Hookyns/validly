using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Validly.Extensions.Validators.Strings;

namespace Validly.Tests;

[Validatable]
partial class JsonNamedObject
{
	[MinLength(5)]
	[JsonPropertyName("username")]
	public required string UserName { get; init; }

	[MinLength(5)]
	public required string FirstName { get; init; }

	[MinLength(5)]
	[DisplayName("Last name")]
	public required string LastName { get; init; }

	[MinLength(5)]
	[JsonPropertyName("weird/~name\"")]
	public required string Weird { get; init; }
}

[Validatable]
partial class JsonNamedRootObject
{
	[JsonPropertyName("parent_json")]
	public required JsonNamedNestedObject Parent { get; init; }

	public required JsonNamedNestedObject OtherParent { get; init; }
}

[Validatable]
partial class JsonNamedNestedObject
{
	[MinLength(5)]
	public required string ChildProp { get; init; }

	[MinLength(5)]
	[JsonPropertyName("explicit_child")]
	public required string ExplicitChild { get; init; }
}

public class JsonPointerNamingTests
{
	private static readonly JsonNamedObject InvalidObject = new()
	{
		UserName = "abc",
		FirstName = "abc",
		LastName = "abc",
		Weird = "abc",
	};

	private static Dictionary<string, string?> GetPointers(ValidationResult result, JsonNamingPolicy? namingPolicy)
	{
		return result
			.GetProblemDetails(namingPolicy)
			.Errors.Where(x => x.FieldName is not null)
			.ToDictionary(x => x.Pointer, x => x.FieldName);
	}

	[Fact]
	public async Task NoPolicy_UsesJsonPropertyNameOrClrName()
	{
		using var result = await InvalidObject.ValidateAsync();

		var pointers = GetPointers(result, null);

		Assert.Equal(nameof(JsonNamedObject.UserName), pointers["#/username"]);
		Assert.Equal(nameof(JsonNamedObject.FirstName), pointers["#/FirstName"]);
		Assert.Equal("Last name", pointers["#/LastName"]);
	}

	[Fact]
	public async Task CamelCasePolicy_JsonPropertyNameTakesPrecedence()
	{
		using var result = await InvalidObject.ValidateAsync();

		var pointers = GetPointers(result, JsonNamingPolicy.CamelCase);

		Assert.Equal(4, pointers.Count);
		Assert.Equal(nameof(JsonNamedObject.UserName), pointers["#/username"]);
		Assert.Equal(nameof(JsonNamedObject.FirstName), pointers["#/firstName"]);
		Assert.Equal("Last name", pointers["#/lastName"]);
	}

	[Fact]
	public async Task SnakeCasePolicy()
	{
		using var result = await InvalidObject.ValidateAsync();

		var pointers = GetPointers(result, JsonNamingPolicy.SnakeCaseLower);

		Assert.Contains("#/username", pointers.Keys);
		Assert.Contains("#/first_name", pointers.Keys);
		Assert.Contains("#/last_name", pointers.Keys);
	}

	[Fact]
	public async Task PointerSegmentsAreEscaped()
	{
		using var result = await InvalidObject.ValidateAsync();

		var pointers = GetPointers(result, JsonNamingPolicy.CamelCase);

		Assert.Equal(nameof(JsonNamedObject.Weird), pointers["#/weird~1~0name\""]);
	}

	[Fact]
	public async Task NestedProperties_EachSegmentIsResolved()
	{
		var root = new JsonNamedRootObject
		{
			Parent = new() { ChildProp = "abc", ExplicitChild = "abc" },
			OtherParent = new() { ChildProp = "abc", ExplicitChild = "abc" },
		};

		using var result = await root.ValidateAsync();

		var pointers = GetPointers(result, JsonNamingPolicy.CamelCase);

		Assert.Equal(4, pointers.Count);
		Assert.Contains("#/parent_json/childProp", pointers.Keys);
		Assert.Contains("#/parent_json/explicit_child", pointers.Keys);
		Assert.Contains("#/otherParent/childProp", pointers.Keys);
		Assert.Contains("#/otherParent/explicit_child", pointers.Keys);

		// CLR path is not affected
		Assert.Contains(
			$"{nameof(JsonNamedRootObject.Parent)}/{nameof(JsonNamedNestedObject.ChildProp)}",
			result.Properties.Select(x => x.PropertyPath)
		);
	}

	[Fact]
	public async Task ProblemDetailsJson_IsValidJsonWithJsonPointers()
	{
		using var result = await InvalidObject.ValidateAsync();

		using var document = JsonDocument.Parse(result.GetProblemDetailsJson(JsonNamingPolicy.CamelCase));

		var errors = document
			.RootElement.GetProperty("errors")
			.EnumerateArray()
			.ToDictionary(x => x.GetProperty("pointer").GetString()!, x => x.GetProperty("fieldName").GetString());

		Assert.Equal(4, errors.Count);
		Assert.Equal(nameof(JsonNamedObject.UserName), errors["#/username"]);
		Assert.Equal(nameof(JsonNamedObject.FirstName), errors["#/firstName"]);
		Assert.Equal("Last name", errors["#/lastName"]);
		Assert.Equal(nameof(JsonNamedObject.Weird), errors["#/weird~1~0name\""]);
	}
}
