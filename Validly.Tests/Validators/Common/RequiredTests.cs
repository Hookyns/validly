using Validly.Validators;

namespace Validly.Tests.Validators.Common;

[Validatable]
partial class RequiredStringTestObject
{
	[Required]
	public required string Value { get; init; }
}

[Validatable]
partial class RequiredNullableValueTypeTestObject
{
	[Required]
	public required int? Value { get; init; }
}

[Validatable]
partial class RequiredNullableIntTypeTestObject
{
	[Required]
	public required int? Value { get; init; }
}

public class RequiredTests
{
	[Fact]
	public void StringValue_IsValid()
	{
		var result = new RequiredStringTestObject { Value = "test" }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void StringValue_IsInvalid()
	{
		var result = new RequiredStringTestObject { Value = "" }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void NullableValueType_IsValid()
	{
		var result = new RequiredNullableValueTypeTestObject { Value = 123 }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void NullableValueType_IsInvalid()
	{
		var result = new RequiredNullableValueTypeTestObject { Value = null }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void ValueType_IsValid()
	{
		var result = new RequiredNullableIntTypeTestObject { Value = 123 }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void ValueType_IsInvalid()
	{
		var result = new RequiredNullableIntTypeTestObject { Value = default }.Validate();
		Assert.False(result.IsSuccess);
	}
}
