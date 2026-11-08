using Validly.Validators;

namespace Validly.Tests;

[Validatable]
partial class RootObject
{
	public required string RootProp { get; init; }

	public required NestedObject NestedObject { get; init; }
}

[Validatable]
partial class NestedObject
{
	[Required]
	[CustomValidation]
	public required string NestedProp { get; init; }

	public bool ValidationExecuted { get; private set; }

	Validation INestedObjectCustomValidation.ValidateNestedProp()
	{
		ValidationExecuted = true;
		return Validation.Error(string.Empty, string.Empty);
	}
}

public class NestedValidatableObjectPropertyTests
{
	[Fact]
	public async Task NestedProperty_Invalid()
	{
		var root = new RootObject
		{
			NestedObject = new() { NestedProp = "foo" },
			RootProp = "root",
		};

		var result = await root.ValidateAsync();
		Assert.False(result.IsSuccess);
		Assert.True(root.NestedObject.ValidationExecuted);

		// Single validation message for the nested property
		Assert.Equal(
			$"{nameof(RootObject.NestedObject)}/{nameof(NestedObject.NestedProp)}",
			result.Properties.Single().PropertyPath
		);
	}
}
