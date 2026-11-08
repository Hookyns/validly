using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Tests;

public class JsonNamingPolicyGeneratorTests
{
	private const string ValidatableSource = """
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
	public void SingleDetectedPolicy_IsResolvedAtCompileTime()
	{
		var result = GeneratorDriverHelper.Run(ValidatableSource + DetectionSource("SnakeCaseLower"));

		Assert.Contains("\"user_name\"", GetGeneratedSource(result));
	}

	[Fact]
	public void AmbiguousDetectedPolicies_FallBackToRuntime()
	{
		var result = GeneratorDriverHelper.Run(
			ValidatableSource + DetectionSource("SnakeCaseLower") + DetectionSource("CamelCase", "Other")
		);

		string generated = GetGeneratedSource(result);
		Assert.DoesNotContain("\"user_name\"", generated);
		Assert.DoesNotContain("\"userName\"", generated);
	}

	private static string DetectionSource(string policy, string className = "Setup") =>
		$$"""

		public static class {{className}}
		{
			public static System.Text.Json.JsonSerializerOptions Create() => new()
			{
				PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.{{policy}},
			};
		}
		""";

	private static string GetGeneratedSource(GeneratorRunResult result) =>
		string.Join("\n", result.GeneratedSources.Select(source => source.SourceText.ToString()));
}
