using Validly.Extensions.Validators.Strings;

namespace Validly.Tests.Validators.Strings;

[Validatable]
partial record PasswordNoRequirementsTestObject
{
	[Password(8)]
	public required string Password { get; init; }
}

[Validatable]
partial record PasswordRequireSpecialCharTestObject
{
	[Password(
		minLength: 8,
		numberOfRequiredSpecialCharacters: 1,
		numberOfRequiredUpperCaseCharacters: 0,
		numberOfRequiredDigitCharacters: 0)]
	public required string Password { get; init; }
}

[Validatable]
partial record PasswordRequireSpecialAndUpperTestObject
{
	[Password(
		minLength: 8,
		numberOfRequiredSpecialCharacters: 1,
		numberOfRequiredUpperCaseCharacters: 1,
		numberOfRequiredDigitCharacters: 0)]
	public required string Password { get; init; }
}

[Validatable]
partial record PasswordRequireAllTestObject
{
	[Password(
		minLength: 8,
		numberOfRequiredSpecialCharacters: 1,
		numberOfRequiredUpperCaseCharacters: 1,
		numberOfRequiredDigitCharacters: 1)]
	public required string Password { get; init; }
}

public class PasswordTests
{
	[Theory]
	[InlineData("password")]
	[InlineData("PASSWORD1")]
	[InlineData("12345678")]
	[InlineData("!@#$%^&*")]
	[InlineData("Mixed!Pass")]
	public void ValidNoRequirementsPasswordTest(string password)
	{
		var value = new PasswordNoRequirementsTestObject { Password = password };

		using var result = value.Validate();

		Assert.True(result.IsSuccess);
	}

	[Theory]
	[InlineData("short")]
	[InlineData("hi")]
	[InlineData("")]
	[InlineData(null)]
	public void InvalidNoRequirementsPasswordTest(string? password)
	{
		var value = new PasswordNoRequirementsTestObject { Password = password! };

		using var result = value.Validate();

		Assert.False(result.IsSuccess);
	}

	[Theory]
	[InlineData("password!")]
	[InlineData("12345678$")]
	[InlineData("mixedcase@")]
	[InlineData("ALLCAPS#")]
	[InlineData("lowercase%")]
	public void ValidRequireSpecialCharPasswordWithTest(string password)
	{
		var value = new PasswordRequireSpecialCharTestObject { Password = password };

		using var result = value.Validate();

		Assert.True(result.IsSuccess);
	}

	[Theory]
	[InlineData("password")]
	[InlineData("12345678")]
	[InlineData("noSpecial123")]
	[InlineData("short!")]
	[InlineData("")]
	[InlineData(null)]
	public void InvalidRequireSpecialCharPasswordTest(string? password)
	{
		var value = new PasswordRequireSpecialCharTestObject { Password = password! };

		using var result = value.Validate();

		Assert.False(result.IsSuccess);
	}
	[Theory]
	[InlineData("Password!")]
	[InlineData("MyPass$word")]
	[InlineData("SecureP@ss")]
	[InlineData("Aa!aaaaa")]
	[InlineData("WithUpper#Here")]
	public void ValidRequireSpecialAndUpperPasswordTest(string password)
	{
		var value = new PasswordRequireSpecialAndUpperTestObject { Password = password };

		using var result = value.Validate();

		Assert.True(result.IsSuccess);
	}

	[Theory]
	[InlineData("password!")]
	[InlineData("Password")]
	[InlineData("pass!")]
	[InlineData("")]
	[InlineData(null)]
	[InlineData("p@ssw0rd")]
	[InlineData("UPPERCASE")]
	[InlineData("lowercase!")]
	public void InvalidRequireSpecialAndUpperPasswordTest(string? password)
	{
		var value = new PasswordRequireSpecialAndUpperTestObject { Password = password! };

		using var result = value.Validate();

		Assert.False(result.IsSuccess);
	}

	[Theory]
	[InlineData("Password1!")]
	[InlineData("MyPass123$")]
	[InlineData("SecureP@ss1")]
	[InlineData("Aa1!aaaa")]
	[InlineData("!Q2w3e4r5")]
	[InlineData("C0mpl3x#Pwd")]
	public void ValidRequireAllPasswordTest(string password)
	{
		var value = new PasswordRequireAllTestObject { Password = password };

		using var result = value.Validate();

		Assert.True(result.IsSuccess);
	}

	[Theory]
	[InlineData("password1!")]
	[InlineData("Password1")]
	[InlineData("Password!")]
	[InlineData("short1!")]
	[InlineData("")]
	[InlineData(null)]
	[InlineData("p@ssw0rd")]
	[InlineData("nouppercase1!")]
	[InlineData("NOUPPERCASE!")]
	[InlineData("PASSWORD1")]
	[InlineData("Missing!")]
	public void InvalidRequireAllPasswordTest(string? password)
	{
		var value = new PasswordRequireAllTestObject { Password = password! };

		using var result = value.Validate();

		Assert.False(result.IsSuccess);
	}
}