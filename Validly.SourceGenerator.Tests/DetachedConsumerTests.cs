using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Validly.SourceGenerator.Tests;

/// <summary>
/// GitHub #23: a project using a validatable domain object (but not validating it) must not need a reference to Validly
/// </summary>
public class DetachedConsumerTests
{
	private const string DomainSource = """
		using System;
		using System.Collections.Generic;
		using System.Threading;
		using System.Threading.Tasks;
		using Validly;
		using Validly.Validators;

		namespace Domain;

		[Validatable(Detached = DETACHED)]
		public partial class User
		{
			[Required]
			public string? Name { get; set; }
		}
		""";

	private const string ConsumerSource = """
		namespace Infrastructure;

		public static class UserStore
		{
			public static Domain.User Create(string name) => new Domain.User { Name = name };

			public static string? GetName(Domain.User user) => user.Name;

			// Overload resolution has to check the interfaces of User (this is where CS0012 appears)
			public static string Describe(Domain.User user) => Format(user);

			private static string Format(object value) => "object";

			private static string Format(System.IFormattable value) => "formattable";
		}
		""";

	[Theory]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void ConsumerWithoutValidlyReference_CompilesOnlyWithDetachedObject(bool detached, bool expectCs0012)
	{
		var domainImage = CompileDomain(detached);

		// Consumer references the domain assembly, not Validly
		var consumer = CSharpCompilation.Create(
			"Infrastructure",
			[CSharpSyntaxTree.ParseText(ConsumerSource)],
			PlatformReferences().Append(MetadataReference.CreateFromImage(domainImage)),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);

		var errors = consumer.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

		Assert.Equal(expectCs0012, errors.Any(d => d.Id == "CS0012"));

		if (!expectCs0012)
		{
			Assert.Empty(errors);
		}
	}

	private static byte[] CompileDomain(bool detached)
	{
		var compilation = CSharpCompilation.Create(
			"Domain",
			[CSharpSyntaxTree.ParseText(DomainSource.Replace("DETACHED", detached ? "true" : "false"), new CSharpParseOptions(LanguageVersion.Latest))],
			PlatformReferences().Append(MetadataReference.CreateFromFile(typeof(ValidatableAttribute).Assembly.Location)),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
		);

		CSharpGeneratorDriver
			.Create([new ValidatableSourceGenerator().AsSourceGenerator()], parseOptions: new CSharpParseOptions(LanguageVersion.Latest))
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

		using var stream = new MemoryStream();
		var result = output.Emit(stream);
		Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));

		return stream.ToArray();
	}

	private static ImmutableArray<MetadataReference> PlatformReferences() =>
		((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Where(path => !Path.GetFileName(path).StartsWith("Validly", StringComparison.Ordinal))
			.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.ToImmutableArray();
}
