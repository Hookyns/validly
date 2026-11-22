using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Tests;

public class JsonNamingPolicyGeneratorTests
{
	private const string ValidatableSource = """
		using System.Text.Json;
		using Validly;
		using Validly.Extensions.Validators.Strings;

		namespace Test;

		[Validatable]
		public partial class Request
		{
			[MinLength(5)]
			public required string UserName { get; init; }
		}
		""";

	[Fact]
	public void UnknownConfiguredPolicy_ReportsDiagnostic()
	{
		var result = GeneratorDriverHelper.Run(ValidatableSource, "<JsonNamingPolicy>PascalCase</JsonNamingPolicy>");

		var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VLY0100");
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Contains("PascalCase", diagnostic.GetMessage());
	}

	[Fact]
	public void KnownConfiguredPolicy_DoesNotReportDiagnostic()
	{
		var result = GeneratorDriverHelper.Run(ValidatableSource, "<JsonNamingPolicy>camelCase</JsonNamingPolicy>");

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "VLY0100");
		Assert.Contains("\"userName\"", GetGeneratedSource(result));
	}

	[Fact]
	public void HttpJsonOptionsPolicy_IsResolvedAtCompileTime()
	{
		var result = RunChecked(ValidatableSource + HttpOptionsSource("JsonNamingPolicy.SnakeCaseLower"));

		Assert.Contains("\"user_name\"", GetGeneratedSource(result));
	}

	[Fact]
	public void MvcJsonOptionsPolicy_IsResolvedAtCompileTime()
	{
		var result = RunChecked(ValidatableSource + MvcOptionsSource("JsonNamingPolicy.KebabCaseLower"));

		Assert.Contains("\"user-name\"", GetGeneratedSource(result));
	}

	[Fact]
	public void UnrelatedSerializerOptions_AreIgnored()
	{
		var result = RunChecked(ValidatableSource + UnrelatedOptionsSource("SnakeCaseLower"));

		AssertNothingResolved(result);
	}

	[Fact]
	public void UnrelatedSerializerOptions_DoNotMakeHttpPolicyAmbiguous()
	{
		var result = RunChecked(
			ValidatableSource
				+ HttpOptionsSource("JsonNamingPolicy.CamelCase")
				+ UnrelatedOptionsSource("SnakeCaseLower")
		);

		string generated = GetGeneratedSource(result);
		Assert.Contains("\"userName\"", generated);
		Assert.DoesNotContain("\"user_name\"", generated);
	}

	[Theory]
	[InlineData("new CustomPolicy()")]
	[InlineData("null")]
	[InlineData("System.Environment.TickCount > 0 ? JsonNamingPolicy.CamelCase : JsonNamingPolicy.SnakeCaseLower")]
	public void UnknownHttpPolicy_FallsBackToRuntime(string policy)
	{
		// The MVC options use a built-in policy, but the HTTP ones can't be resolved; nothing can be assumed
		var result = RunChecked(
			ValidatableSource + HttpOptionsSource(policy) + MvcOptionsSource("JsonNamingPolicy.CamelCase")
		);

		AssertNothingResolved(result);
	}

	[Fact]
	public void DifferentHttpAndMvcPolicies_FallBackToRuntime()
	{
		var result = RunChecked(
			ValidatableSource
				+ HttpOptionsSource("JsonNamingPolicy.SnakeCaseLower")
				+ MvcOptionsSource("JsonNamingPolicy.CamelCase")
		);

		AssertNothingResolved(result);
	}

	[Fact]
	public void SameHttpAndMvcPolicies_AreResolvedAtCompileTime()
	{
		var result = RunChecked(
			ValidatableSource
				+ HttpOptionsSource("JsonNamingPolicy.CamelCase")
				+ MvcOptionsSource("JsonNamingPolicy.CamelCase")
		);

		Assert.Contains("\"userName\"", GetGeneratedSource(result));
	}

	[Fact]
	public void ConfiguredPolicy_WinsOverDetectedPolicy()
	{
		var result = RunChecked(
			ValidatableSource + HttpOptionsSource("JsonNamingPolicy.CamelCase"),
			"<JsonNamingPolicy>snake_case_lower</JsonNamingPolicy>"
		);

		string generated = GetGeneratedSource(result);
		Assert.Contains("\"user_name\"", generated);
		Assert.DoesNotContain("\"userName\"", generated);
	}

	[Fact]
	public void ConfiguredNone_DisablesDetection()
	{
		var result = RunChecked(
			ValidatableSource + HttpOptionsSource("JsonNamingPolicy.CamelCase"),
			"<JsonNamingPolicy>none</JsonNamingPolicy>"
		);

		AssertNothingResolved(result);
	}

	/// <summary>
	/// Runs the generator and makes sure the test source itself compiles, so a broken setup can't pass as "not detected"
	/// </summary>
	private static GeneratorRunResult RunChecked(string source, string? validlyConfiguration = null)
	{
		var result = GeneratorDriverHelper.Run(source, validlyConfiguration, out Compilation compilation);

		Assert.Empty(
			compilation
				.GetDiagnostics()
				.Where(d => d.Severity == DiagnosticSeverity.Error && d.Location.SourceTree == compilation.SyntaxTrees.First())
		);

		return result;
	}

	private static void AssertNothingResolved(GeneratorRunResult result)
	{
		string generated = GetGeneratedSource(result);
		Assert.Contains("\"UserName\"", generated);
		Assert.DoesNotContain("\"user_name\"", generated);
		Assert.DoesNotContain("\"userName\"", generated);
		Assert.DoesNotContain("\"user-name\"", generated);
	}

	private static string HttpOptionsSource(string policy) =>
		$$"""

		public static class HttpSetup
		{
			public static void Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
			{
				Microsoft.Extensions.DependencyInjection.HttpJsonServiceExtensions.ConfigureHttpJsonOptions(
					services,
					options => options.SerializerOptions.PropertyNamingPolicy = {{policy}}
				);
			}
		}

		public sealed class CustomPolicy : System.Text.Json.JsonNamingPolicy
		{
			public override string ConvertName(string name) => name;
		}
		""";

	private static string MvcOptionsSource(string policy) =>
		$$"""

		public static class MvcSetup
		{
			public static void Configure(Microsoft.Extensions.DependencyInjection.IMvcBuilder builder)
			{
				Microsoft.Extensions.DependencyInjection.MvcCoreMvcBuilderExtensions.AddJsonOptions(
					builder,
					options =>
					{
						options.JsonSerializerOptions.PropertyNamingPolicy = {{policy}};
					}
				);
			}
		}
		""";

	private static string UnrelatedOptionsSource(string policy) =>
		$$"""

		public static class ClientSetup
		{
			public static System.Text.Json.JsonSerializerOptions Create()
			{
				var options = new System.Text.Json.JsonSerializerOptions
				{
					PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.{{policy}},
				};
				options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.{{policy}};
				return options;
			}
		}
		""";

	private static string GetGeneratedSource(GeneratorRunResult result) =>
		string.Join("\n", result.GeneratedSources.Select(source => source.SourceText.ToString()));
}
