using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Validly.SourceGenerator.Tests;

/// <summary>
/// Runs the Validly generator over in-memory sources
/// </summary>
internal static class GeneratorDriverHelper
{
	private static readonly Lazy<ImmutableArray<MetadataReference>> References = new(() =>
	{
		var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);

		return trustedAssemblies
			.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.Append(MetadataReference.CreateFromFile(typeof(ValidatableAttribute).Assembly.Location))
			.Append(MetadataReference.CreateFromFile(typeof(Validly.Extensions.Validators.Strings.MinLengthAttribute).Assembly.Location))
			.ToImmutableArray();
	});

	public static GeneratorRunResult Run(string source, string? validlyConfiguration = null)
	{
		return Run(source, validlyConfiguration, out _);
	}

	public static GeneratorRunResult Run(string source, string? validlyConfiguration, out Compilation outputCompilation)
	{
		var compilation = CSharpCompilation.Create(
			"GeneratorTests",
			[CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
			References.Value,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
		);

		var optionsProvider = new TestAnalyzerConfigOptionsProvider(
			validlyConfiguration is null
				? new Dictionary<string, string>()
				: new Dictionary<string, string> { ["build_property.ValidlyConfiguration"] = validlyConfiguration }
		);

		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[new ValidatableSourceGenerator().AsSourceGenerator()],
			optionsProvider: optionsProvider,
			parseOptions: new CSharpParseOptions(LanguageVersion.Latest)
		);

		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out _);

		return driver.GetRunResult().Results.Single();
	}

	private sealed class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions)
		: AnalyzerConfigOptionsProvider
	{
		public override AnalyzerConfigOptions GlobalOptions { get; } = new TestAnalyzerConfigOptions(globalOptions);

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => TestAnalyzerConfigOptions.Empty;

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => TestAnalyzerConfigOptions.Empty;
	}

	private sealed class TestAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
	{
		public static readonly TestAnalyzerConfigOptions Empty = new(new Dictionary<string, string>());

		public override bool TryGetValue(string key, out string value) => options.TryGetValue(key, out value!);
	}
}
