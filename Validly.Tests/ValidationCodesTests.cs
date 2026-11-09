using System.Text.Json;
using Validly.Validators;

namespace Validly.Tests;

[Validatable(NoAutoValidators = true)]
public partial class CodesRequest
{
	[LengthBetween(5, 20)]
	public string? UserName { get; set; }

	[Required]
	public string? Email { get; set; }

	[Between(18, 99)]
	public int Age { get; set; }

	[MinCollectionSize(1)]
	public List<string>? Tags { get; set; }

	[Match("^[A-Z]+$")]
	public string? Code { get; set; }

	[Password(minLength: 4, numberOfRequiredDigitCharacters: 3)]
	public string? Password { get; set; }
}

[Validatable(NoAutoValidators = true)]
public partial class CustomNamedArgsRequest
{
	[Validly.Validators.CustomValidation]
	public decimal Balance { get; set; }

	public ValidationMessage? ValidateBalance() =>
		Balance >= 10
			? null
			: new ValidationMessage("Balance must be at least {0}.", "BusinessRules.InsufficientBalance", 10m)
			{
				NamedArgs = [new("minimum", 10m), new("currency", "EUR \"€\"")],
			};
}

public class ValidationCodesTests
{
	private static CodesRequest CreateInvalid() =>
		new()
		{
			UserName = "abc",
			Email = null,
			Age = 5,
			Tags = [],
			Code = "abc",
			Password = "abcd",
		};

	[Fact]
	public void DefaultMode_UsesNewCodes()
	{
		Assert.False(ValidlyOptions.UseLegacyValidationCodes);

		using var result = CreateInvalid().Validate();
		var codes = result.Properties.Where(p => !p.IsSuccess).ToDictionary(p => p.PropertyPath, p => p.Messages.Single().Code);

		Assert.Equal("Validations.LengthBetween", codes["UserName"]);
		Assert.Equal("Validations.Required", codes["Email"]);
		Assert.Equal("Validations.Between", codes["Age"]);
		Assert.Equal("Validations.MinCollectionSize", codes["Tags"]);
		Assert.Equal("Validations.Match", codes["Code"]);
		Assert.Equal("Validations.Password.Digit", codes["Password"]);
	}

	[Fact]
	public void BuiltInValidators_ProvideNamedArgs()
	{
		using var result = CreateInvalid().Validate();
		var namedArgs = result
			.Properties.Where(p => !p.IsSuccess)
			.ToDictionary(p => p.PropertyPath, p => p.Messages.Single().NamedArgs?.ToDictionary(a => a.Key, a => a.Value));

		Assert.Equal(new Dictionary<string, object?> { ["min"] = 5, ["max"] = 20 }, namedArgs["UserName"]);
		Assert.Null(namedArgs["Email"]);
		Assert.Equal(new Dictionary<string, object?> { ["min"] = 18m, ["max"] = 99m }, namedArgs["Age"]);
		Assert.Equal(new Dictionary<string, object?> { ["min"] = 1 }, namedArgs["Tags"]);
		Assert.Equal(new Dictionary<string, object?> { ["pattern"] = "^[A-Z]+$" }, namedArgs["Code"]);
		// The digit requirement, not the upper-case one
		Assert.Equal(new Dictionary<string, object?> { ["min"] = 3 }, namedArgs["Password"]);
	}

	[Fact]
	public void PasswordDigitMessage_UsesTheDigitRequirement()
	{
		using var result = new CodesRequest { Password = "abcd" }.Validate();

		var message = result.Properties.Single(p => p.PropertyPath == "Password").Messages.Single();
		Assert.Equal(3, message.Args.Single());
	}

	[Fact]
	public void ProblemDetailsJson_ContainsCodeAndNamedArgs()
	{
		using var result = CreateInvalid().Validate();

		using var json = JsonDocument.Parse(result.GetProblemDetailsJson());
		var errors = json.RootElement.GetProperty("errors").EnumerateArray().ToArray();
		var userName = errors.Single(e => e.GetProperty("pointer").GetString() == "#/UserName");
		var email = errors.Single(e => e.GetProperty("pointer").GetString() == "#/Email");

		Assert.Equal("Validations.LengthBetween", userName.GetProperty("code").GetString());
		Assert.False(userName.TryGetProperty("resourceKey", out _));
		Assert.Equal(5, userName.GetProperty("namedArgs").GetProperty("min").GetInt32());
		Assert.Equal(20, userName.GetProperty("namedArgs").GetProperty("max").GetInt32());
		Assert.Equal(2, userName.GetProperty("args").GetArrayLength());

		// No named arguments, no member
		Assert.False(email.TryGetProperty("namedArgs", out _));
	}

	[Fact]
	public void CustomMessage_NamedArgs_InProblemDetails()
	{
		using var result = new CustomNamedArgsRequest { Balance = 1 }.Validate();

		var detail = result.GetProblemDetails().Errors.Single();
		Assert.Equal("BusinessRules.InsufficientBalance", detail.Code);
		Assert.True(detail.NamedArgs!.ContainsKey("minimum"));

		using var json = JsonDocument.Parse(result.GetProblemDetailsJson());
		var namedArgs = json.RootElement.GetProperty("errors")[0].GetProperty("namedArgs");
		Assert.Equal(10m, namedArgs.GetProperty("minimum").GetDecimal());
		Assert.Equal("EUR \"€\"", namedArgs.GetProperty("currency").GetString());
	}

	[Fact]
	public void ArgsJson_IsLazy_AndFollowsWithExpressions()
	{
		var message = new ValidationMessage("{0}", "Code", 1);
		Assert.Equal("1", message.ArgsJson);

		var changed = message with { Args = [2, "x"] };
		Assert.Equal("2, \"x\"", changed.ArgsJson);
		Assert.Equal(string.Empty, changed.NamedArgsJson);
	}

	[Fact]
	public void Equality_IsNotAffectedByCachedJson()
	{
		object?[] args = [1];
		var first = new ValidationMessage("{0}", "Code", args);
		var second = new ValidationMessage("{0}", "Code", args);

		_ = first.ArgsJson;

		Assert.Equal(first, second);
		Assert.Equal(first.GetHashCode(), second.GetHashCode());
	}

#pragma warning disable CS0618 // ResourceKey is obsolete
	[Fact]
	public void ResourceKey_IsAliasOfCode()
	{
		var message = new ValidationMessage("Message", "Some.Code");

		Assert.Equal("Some.Code", message.ResourceKey);
	}
#pragma warning restore CS0618
}
