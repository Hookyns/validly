using Validly.Validators;

namespace Validly.Tests.Validators.Enums;

public enum SomeEnum
{
	One = 1,
	Three = 3,
	Five = 5,
}

[Validatable]
partial class InEnumTestObject
{
	[InEnum]
	public required SomeEnum EnumValue { get; init; }
}

public class InEnumTests
{
	[Fact]
	public void ValidInEnumTest()
	{
		var val = new InEnumTestObject { EnumValue = SomeEnum.One };

		using var result = val.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void DefaultValueTest()
	{
		var val = new InEnumTestObject { EnumValue = default };

		using var result = val.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void InvalidValueTest()
	{
		var val = new InEnumTestObject { EnumValue = (SomeEnum)99 };

		using var result = val.Validate();
		Assert.False(result.IsSuccess);
	}
}
