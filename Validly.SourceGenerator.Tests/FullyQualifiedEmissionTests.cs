namespace Validly.SourceGenerator.Tests;

/// <summary>
/// The generated code must not depend on the usings of the user's file (nor on the SDK's implicit usings);
/// it is also emitted into files of third-party generators
/// </summary>
public class FullyQualifiedEmissionTests
{
	private const string Source = """
		using Validly;
		using Validly.Validators;
		using Services.Words;
		using Options;

		namespace Services.Words
		{
			public interface IWordList
			{
				bool Contains(string value);
			}
		}

		namespace Options
		{
			public enum Mode { Lenient, Strict }

			[Validator]
			[ValidatorDescription("checks mode")]
			[System.AttributeUsage(System.AttributeTargets.Property)]
			public sealed class ModeCheckAttribute : System.Attribute
			{
				private readonly Mode _mode;

				public ModeCheckAttribute(Mode mode) => _mode = mode;

				public ValidationMessage? IsValid(string? value) =>
					_mode == Mode.Strict && value == "x" ? new ValidationMessage("Strict", "Mode.Strict") : null;
			}
		}

		namespace App
		{
			[Validatable]
			public partial class Request
			{
				[Match(@"^\d{3}-[a-z]+$")]
				public string? Code { get; set; }

				[ModeCheck(Mode.Strict)]
				public string? Value { get; set; }

				[Validly.Validators.CustomValidation]
				public string? Word { get; set; }

				public ValidationMessage? ValidateWord(IWordList words) =>
					words.Contains(Word ?? string.Empty) ? null : new ValidationMessage("Unknown", "Word.Unknown");
			}
		}
		""";

	[Fact]
	public void GeneratedCode_DoesNotDependOnUsings()
	{
		var result = GeneratorAssert.CompilesWithoutErrors(Source);
		string generated = result.GeneratedSources.Single().SourceText.ToString();

		Assert.DoesNotContain("using Services.Words;", generated);
		Assert.Contains("global::Services.Words.IWordList", generated);
		Assert.Contains("(global::Options.Mode)1", generated);
		Assert.Contains("\"^\\\\d{3}-[a-z]+$\"", generated);
	}
}
