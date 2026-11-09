using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Validly.SourceGenerator.Analyzers;

namespace Validly.SourceGenerator.Tests;

public class ValidatableObjectAnalyzerTests
{
	private const string Usings = """
		using System;
		using System.Collections.Generic;
		using System.Threading;
		using System.Threading.Tasks;
		using Validly;
		using Validly.Validators;

		""";

	[Fact]
	public void SupportedShapes_NoDiagnostics()
	{
		var diagnostics = Analyze(
			"""
			public interface IClock { DateTime Now { get; } }

			[Validatable]
			public partial class Ok
			{
				[CustomValidation]
				public string? A { get; set; }

				[CustomValidation]
				public string? B { get; set; }

				public ValidationMessage? ValidateA(IClock clock, CancellationToken ct) => null;

				public async IAsyncEnumerable<ValidationMessage> ValidateB(ValidationContext context)
				{
					await Task.Yield();
					yield break;
				}

				private Task<ValidationResult?> BeforeValidate(ValidationResult result) => Task.FromResult<ValidationResult?>(null);

				private IEnumerable<ValidationMessage> AfterValidate(IClock clock) { yield break; }
			}

			[Validator]
			[AttributeUsage(AttributeTargets.Property)]
			public sealed class OkValidatorAttribute : Attribute
			{
				public ValueTask<Validation> IsValid(string? value, IClock clock) => new(Validation.Success());
			}
			"""
		);

		Assert.Empty(diagnostics);
	}

	[Theory]
	[InlineData("[Validatable] public class NotPartial { }", "VLY0010")]
	[InlineData("public partial class Outer { [Validatable] public partial class Nested { } }", "VLY0011")]
	[InlineData("[Validatable] public partial class H { private bool AfterValidate() => true; }", "VLY0012")]
	[InlineData("[Validatable] public partial class H { private List<ValidationMessage> AfterValidate() => new(); }", "VLY0012")]
	[InlineData("[Validatable] public partial class H { private Task<IEnumerable<ValidationMessage>> BeforeValidate() => null!; }", "VLY0012")]
	[InlineData("[Validatable] public partial class H { private void BeforeValidate(int count) { } }", "VLY0013")]
	[InlineData("[Validatable] public partial class H { private void BeforeValidate(ref ValidationContext context) { } }", "VLY0013")]
	[InlineData("[Validatable] public partial class C { [CustomValidation] public string? Name { get; set; } }", "VLY0014")]
	[InlineData("[Validatable] public partial class C { [CustomValidation] public string? Name { get; set; } private ValidationMessage? ValidateName() => null; }", "VLY0015")]
	[InlineData("[Validatable] public partial class C { [CustomValidation] public string? Name { get; set; } public bool ValidateName() => true; }", "VLY0016")]
	[InlineData("[Validatable] public partial class C { [CustomValidation] public string? Name { get; set; } public Task ValidateName() => Task.CompletedTask; }", "VLY0016")]
	[InlineData("[Validator] public sealed class NoIsValidAttribute : Attribute { }", "VLY0017")]
	[InlineData("[Validator] public sealed class BoolValidatorAttribute : Attribute { public bool IsValid(string? value) => true; }", "VLY0016")]
	public void UnsupportedShapes_AreReported(string source, string expectedId)
	{
		var diagnostics = Analyze(source);

		Assert.Contains(diagnostics, d => d.Id == expectedId);
	}

	private static ImmutableArray<Diagnostic> Analyze(string source)
	{
		var trusted = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
		var compilation = CSharpCompilation.Create(
			"AnalyzerTests",
			[CSharpSyntaxTree.ParseText(Usings + source, new CSharpParseOptions(LanguageVersion.Latest))],
			trusted
				.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
				.Append(MetadataReference.CreateFromFile(typeof(ValidatableAttribute).Assembly.Location)),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
		);

		return compilation
			.WithAnalyzers([new ValidatableObjectAnalyzer()])
			.GetAnalyzerDiagnosticsAsync()
			.GetAwaiter()
			.GetResult();
	}
}
