using Validly.Validators;

namespace Validly.Tests;

[Validatable]
partial class CancellationTokenSupportTestObject
{
	[CustomValidation]
	public required string SomeValue { get; set; }

	private void BeforeValidate(CancellationToken ct) { }

	private ValidationResult AfterValidate(CancellationToken ct)
	{
		return new ValidationResult([BeforeValidateTests.Message]);
	}

	public Validation ValidateSomeValue(CancellationToken ct)
	{
		return Validation.Success();
	}
}

public class CancellationTokenSupportTests
{
	public static readonly ValidationMessage Message = new("Test error", "message.key");

	[Fact]
	public void DirectValidationResult_Test()
	{
		var request = new BeforeValidateEnumerableReturnType { SomeValue = "test" };

		using var result = request.Validate();
		Assert.False(result.IsSuccess);
		Assert.Collection(result.Global, message => Assert.Equal(Message, message));
	}

	[Fact]
	public void EnumerableReturnType_Test()
	{
		var request = new BeforeValidateEnumerableReturnType { SomeValue = "test" };

		using var result = request.Validate();
		Assert.False(result.IsSuccess);
		Assert.Collection(result.Global, message => Assert.Equal(Message, message));
	}

	[Fact]
	public async Task AsyncEnumerableReturnType_Test()
	{
		var request = new BeforeValidateAsyncEnumerableReturnType { SomeValue = "test" };

		using var result = await request.ValidateAsync();
		Assert.False(result.IsSuccess);
		Assert.Collection(result.Global, message => Assert.Equal(Message, message));
	}
}
