using Validly.Extensions.Validators.Strings;

namespace Validly.Tests.Validators.Strings;

[Validatable]
partial record PhoneNumberTestObject
{
	[PhoneNumber]
	public required string PhoneNumberValue { get; init; }
}

public class PhoneNumberTests
{
	[Theory]
	[InlineData("+79123456")]
	[InlineData("+1234567890")]
	[InlineData("+987654321012345")]
	public void ValidPhoneNumbers_ShouldPass(string number)
	{
		var value = new PhoneNumberTestObject { PhoneNumberValue = number };

		using var result = value.Validate();

		Assert.True(result.IsSuccess);
	}

	[Theory]
	[InlineData("+123456")]
	[InlineData("+1")]
	[InlineData("+1234567890123456")]
	[InlineData("123456789")]
	[InlineData("+012345678")]
	[InlineData("+123abc")]
	[InlineData("+")]
	[InlineData("")]
	public void InvalidPhoneNumbers_ShouldFail(string number)
	{
		var value = new PhoneNumberTestObject { PhoneNumberValue = number };

		using var result = value.Validate();

		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void NullValue_HandledGracefully()
	{
		var value = new PhoneNumberTestObject { PhoneNumberValue = null! };

		using var result = value.Validate();

		Assert.True(result.IsSuccess);
	}
}
