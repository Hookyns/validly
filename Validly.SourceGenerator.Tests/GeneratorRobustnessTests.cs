namespace Validly.SourceGenerator.Tests;

public class GeneratorRobustnessTests
{
	private const string ExitEarlyConfiguration = "<ExitEarly>enable</ExitEarly>";

	[Fact]
	public void ExitEarly_SyncObject_Compiles()
	{
		GeneratorAssert.CompilesWithoutErrors(
			"""
			using Validly;
			using Validly.Validators;

			namespace Test;

			[Validatable]
			public partial class Request
			{
				[MinLength(5)]
				public required string Name { get; init; }
			}
			""",
			ExitEarlyConfiguration
		);
	}

	[Fact]
	public void ExitEarly_SyncObjectWithSyncHooks_Compiles()
	{
		GeneratorAssert.CompilesWithoutErrors(
			"""
			using System.Collections.Generic;
			using Validly;
			using Validly.Validators;

			namespace Test;

			[Validatable]
			public partial class Request
			{
				[MinLength(5)]
				public required string Name { get; init; }

				private ValidationResult? BeforeValidate() => null;

				private IEnumerable<ValidationMessage> AfterValidate() => [];
			}
			""",
			ExitEarlyConfiguration
		);
	}

	[Fact]
	public void ExitEarly_AsyncObjectWithNestedProperty_Compiles()
	{
		GeneratorAssert.CompilesWithoutErrors(
			"""
			using Validly;
			using Validly.Validators;

			namespace Test;

			[Validatable]
			public partial class Request
			{
				[MinLength(5)]
				public required string Name { get; init; }

				public required Child Child { get; init; }

				// Sync hook returning ValidationResult inside an async method
				private ValidationResult? BeforeValidate() => null;
			}

			[Validatable]
			public partial class Child
			{
				[MinLength(5)]
				public required string Value { get; init; }
			}
			""",
			ExitEarlyConfiguration
		);
	}

	[Fact]
	public void ExitEarly_AsyncObjectWithAsyncHooks_Compiles()
	{
		GeneratorAssert.CompilesWithoutErrors(
			"""
			using System.Threading.Tasks;
			using Validly;
			using Validly.Validators;

			namespace Test;

			[Validatable]
			public partial class Request
			{
				[MinLength(5)]
				public required string Name { get; init; }

				private Task<ValidationResult?> BeforeValidate() => Task.FromResult<ValidationResult?>(null);

				private async Task<ValidationMessage?> AfterValidate()
				{
					await Task.Yield();
					return null;
				}
			}
			""",
			ExitEarlyConfiguration
		);
	}

	[Fact]
	public void ValidatorWithServiceDependency_Compiles()
	{
		var result = GeneratorAssert.CompilesWithoutErrors(
			"""
			using System;
			using Validly;
			using Validly.Validators;

			namespace Test;

			public interface IForbiddenWords
			{
				bool Contains(string value);
			}

			[Validator]
			[AttributeUsage(AttributeTargets.Property)]
			public sealed class NotForbiddenAttribute : Attribute
			{
				public ValidationMessage? IsValid(string? value, IForbiddenWords forbiddenWords) =>
					value is not null && forbiddenWords.Contains(value) ? new ValidationMessage("Forbidden.", "Forbidden") : null;
			}

			[Validatable]
			public partial class Request
			{
				[NotForbidden]
				public required string Name { get; init; }
			}
			"""
		);

		string generated = string.Join("\n", result.GeneratedSources.Select(s => s.SourceText.ToString()));
		Assert.Matches(@"GetRequiredService<global::[\w.]*IForbiddenWords>\(serviceProvider\)", generated);
		Assert.Matches(@"IsValid\(Name, serviceIForbiddenWords\w*\)", generated);
	}

	[Fact]
	public void SameNameInDifferentNamespaces_Compiles()
	{
		var result = GeneratorAssert.CompilesWithoutErrors(
			"""
			using Validly;
			using Validly.Validators;

			namespace First
			{
				[Validatable]
				public partial class Request
				{
					[MinLength(5)]
					public required string Name { get; init; }
				}
			}

			namespace Second
			{
				[Validatable]
				public partial class Request
				{
					[MinLength(5)]
					public required string Name { get; init; }
				}
			}
			"""
		);

		Assert.Equal(2, result.GeneratedSources.Length);
		Assert.Contains(result.GeneratedSources, s => s.HintName == "First.Request.Validator.g.cs");
		Assert.Contains(result.GeneratedSources, s => s.HintName == "Second.Request.Validator.g.cs");
	}
}

public class GlobalUsingsTests
{
	[Fact]
	public void GlobalUsingsInTheFile_AreNotCopied()
	{
		// GeneratorAssert prepends global usings to the very same file
		var result = GeneratorAssert.CompilesWithoutErrors(
			"""
			using Validly;
			using Validly.Validators;

			namespace Test;

			[Validatable]
			public partial class Request
			{
				[MinLength(5)]
				public required string Name { get; init; }
			}
			"""
		);

		Assert.DoesNotContain("global using", result.GeneratedSources.Single().SourceText.ToString());
	}
}

public class GeneratorReviewRegressionTests
{
	[Fact]
	public void ServicesWithTheSameSimpleName_AndAttributeArguments_Compile()
	{
		GeneratorAssert.CompilesWithoutErrors(
			"""
			using Validly;
			using Validly.Validators;

			namespace First { public interface IRepo<T> { bool Has(T value); } public interface IStore { } }
			namespace Second { public interface IStore { } }

			namespace App
			{
				[Validator]
				[System.AttributeUsage(System.AttributeTargets.Property)]
				public sealed class MaxFloatAttribute(float max) : System.Attribute
				{
					public string? Label { get; set; }
					public double Ratio { get; set; }
					public ValidationMessage? IsValid(float value) => value > max ? new ValidationMessage(Label ?? "x", "Max") : null;
				}

				[Validatable]
				public partial class Request
				{
					[MaxFloat(1.5f, Label = "a \"b\" c", Ratio = double.NaN)]
					public float Value { get; set; }

					[Validly.Validators.CustomValidation]
					public string? A { get; set; }

					[Validly.Validators.CustomValidation]
					public string? B { get; set; }

					public ValidationMessage? ValidateA(First.IRepo<int> ints, First.IStore store, ExtendableValidationResult result) => null;

					public ValidationMessage? ValidateB(First.IRepo<string> strings, Second.IStore store) => null;
				}
			}
			"""
		);
	}
}
