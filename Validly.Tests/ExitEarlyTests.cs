using Validly.Extensions.Validators.Strings;

namespace Validly.Tests;

[Validatable(UseExitEarly = true)]
public partial class ExitEarlyWithNestedObject
{
	[MinLength(5)]
	public required string First { get; init; }

	[MinLength(5)]
	public required string Second { get; init; }

	public required ExitEarlyNestedObject Nested { get; init; }
}

[Validatable]
public partial class ExitEarlyNestedObject
{
	[MinLength(5)]
	public required string Value { get; init; }
}

[Validatable(UseExitEarly = true)]
public partial class ExitEarlyWithAsyncHook
{
	[MinLength(5)]
	public required string First { get; init; }

	[MinLength(5)]
	public required string Second { get; init; }

	public bool AfterValidateCalled { get; private set; }

	private ValidationResult? BeforeValidate() => null;

	private async Task<ValidationMessage?> AfterValidate()
	{
		await Task.Yield();
		AfterValidateCalled = true;
		return null;
	}
}

public class ExitEarlyTests
{
	[Fact]
	public async Task AsyncObjectWithNestedProperty_ExitsOnFirstInvalidProperty()
	{
		var obj = new ExitEarlyWithNestedObject
		{
			First = "abc",
			Second = "abc",
			Nested = new ExitEarlyNestedObject { Value = "valid value" },
		};

		using var result = await obj.ValidateAsync();

		Assert.False(result.IsSuccess);
		var invalid = Assert.Single(result.Properties, property => !property.IsSuccess);
		Assert.Equal("First", invalid.PropertyPath);
	}

	[Fact]
	public async Task AsyncObjectWithNestedProperty_ValidObject_IsSuccess()
	{
		var obj = new ExitEarlyWithNestedObject
		{
			First = "valid value",
			Second = "valid value",
			Nested = new ExitEarlyNestedObject { Value = "valid value" },
		};

		using var result = await obj.ValidateAsync();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task AsyncHooks_ExitEarly_SkipsTheRest()
	{
		var obj = new ExitEarlyWithAsyncHook { First = "abc", Second = "abc" };

		using var result = await obj.ValidateAsync();

		Assert.False(result.IsSuccess);
		Assert.Single(result.Properties, property => !property.IsSuccess);
		Assert.False(obj.AfterValidateCalled);
	}

	[Fact]
	public async Task AsyncHooks_ValidObject_RunsAfterValidate()
	{
		var obj = new ExitEarlyWithAsyncHook { First = "valid value", Second = "valid value" };

		using var result = await obj.ValidateAsync();

		Assert.True(result.IsSuccess);
		Assert.True(obj.AfterValidateCalled);
	}
}
