using Microsoft.Extensions.DependencyInjection;
using Validly.Extensions.Validators.Common;

namespace Validly.Tests;

[Validatable]
partial class ValidatableInterfaceTestObject
{
	[Required]
	public required string Prop { get; init; }
}

public class ValidatableInterfaceTests
{
	[Fact]
	public async Task IValidatable_Valid()
	{
		var root = new ValidatableInterfaceTestObject
		{
			Prop = "Test",
		};

		IValidatable validatable = root;
		var result = await validatable.ValidateAsync(
			new ServiceCollection().BuildServiceProvider(),
			CancellationToken.None);

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task IValidatable_Invalid()
	{
		var root = new ValidatableInterfaceTestObject
		{
			Prop = null!
		};

		IValidatable validatable = root;
		var result = await validatable.ValidateAsync(
			new ServiceCollection().BuildServiceProvider(),
			CancellationToken.None);

		Assert.False(result.IsSuccess);
	}
}
