using System.Text.Json;
using Validly.Validators;

namespace Validly.Tests.LegacyCodes;

[Validatable]
public partial class LegacyRequest
{
	[LengthBetween(5, 20)]
	public string? UserName { get; set; }

	[Required]
	public string? Email { get; set; }

	[Password(minLength: 10)]
	public string? Password { get; set; }
}

public class LegacyCodesTests
{
	[Fact]
	public void Switch_IsEnabledFromMsBuild()
	{
		Assert.True(ValidlyOptions.UseLegacyValidationCodes);
	}

	[Fact]
	public void BuiltInValidators_UseLegacyCodes()
	{
		using var result = new LegacyRequest { UserName = "abc", Email = null, Password = "short" }.Validate();

		var codes = result.Properties.Where(p => !p.IsSuccess).ToDictionary(p => p.PropertyPath, p => p.Messages.Single().Code);

		Assert.Equal(LegacyValidationCodes.LengthBetween, codes["UserName"]);
		Assert.Equal("Validly.Validations.LengthBetween.Error", codes["UserName"]);
		Assert.Equal(LegacyValidationCodes.Required, codes["Email"]);
		Assert.Equal(LegacyValidationCodes.PasswordLength, codes["Password"]);
	}

	[Fact]
	public void ProblemDetailsJson_ContainsResourceKey_NotCode()
	{
		using var result = new LegacyRequest { UserName = "abc", Email = "e", Password = "long enough password 1!A" }.Validate();

		using var json = JsonDocument.Parse(result.GetProblemDetailsJson());
		var error = json.RootElement.GetProperty("errors")[0];

		Assert.Equal("Validly.Validations.LengthBetween.Error", error.GetProperty("resourceKey").GetString());
		Assert.False(error.TryGetProperty("code", out _));
	}
}
