using System.Text.Json;
using Validly.Naming;

namespace Validly.Tests;

/// <summary>
/// Validly converts the names itself (at compile time, when possible), so the results have to match
/// the System.Text.Json policies exactly.
/// </summary>
public class NamingPolicyParityTests
{
	public static TheoryData<string> Names =>
		new()
		{
			"UserName",
			"Username",
			"userName",
			"username",
			"U",
			"ID",
			"Id",
			"IPAddress",
			"XMLReader",
			"SHA512Hash",
			"Address1",
			"Address_1",
			"My Property",
			"  Leading And Trailing  ",
			"already_snake_case",
			"already-kebab-case",
			"With.Dot",
			"Příliš Žluťoučký",
			"A1B2C3",
			"HTTPSConnection2Pool",
			"_underscorePrefixed",
			"",
		};

	[Theory]
	[MemberData(nameof(Names))]
	public void CamelCase_MatchesSystemTextJson(string name)
	{
		Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(name), PropertyNamingPolicy.CamelCase.ConvertName(name));
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void SnakeCaseLower_MatchesSystemTextJson(string name)
	{
		Assert.Equal(
			JsonNamingPolicy.SnakeCaseLower.ConvertName(name),
			PropertyNamingPolicy.SnakeCaseLower.ConvertName(name)
		);
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void SnakeCaseUpper_MatchesSystemTextJson(string name)
	{
		Assert.Equal(
			JsonNamingPolicy.SnakeCaseUpper.ConvertName(name),
			PropertyNamingPolicy.SnakeCaseUpper.ConvertName(name)
		);
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void KebabCaseLower_MatchesSystemTextJson(string name)
	{
		Assert.Equal(
			JsonNamingPolicy.KebabCaseLower.ConvertName(name),
			PropertyNamingPolicy.KebabCaseLower.ConvertName(name)
		);
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void KebabCaseUpper_MatchesSystemTextJson(string name)
	{
		Assert.Equal(
			JsonNamingPolicy.KebabCaseUpper.ConvertName(name),
			PropertyNamingPolicy.KebabCaseUpper.ConvertName(name)
		);
	}

	[Fact]
	public void CustomPolicy_IsInvoked()
	{
		var policy = PropertyNamingPolicy.Create(name => $"x_{name.ToLowerInvariant()}");

		Assert.Equal("x_username", policy.ConvertName("UserName"));
	}
}
