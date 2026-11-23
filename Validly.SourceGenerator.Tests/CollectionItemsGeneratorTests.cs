namespace Validly.SourceGenerator.Tests;

public class CollectionItemsGeneratorTests
{
	private const string Source = """
		using System.Collections.Immutable;
		using Validly;
		using Validly.Extensions.Validators.Common;

		namespace Test;

		[Validatable]
		public partial record Order
		{
			public List<Line>? Lines { get; init; }
			public Line[]? LineArray { get; init; }
			public ImmutableArray<Line> Immutable { get; init; }
			public HashSet<Line>? Set { get; init; }
			public List<Generic<int>>? GenericLines { get; init; }
			public Dictionary<string, Line>? NotACollectionOfValidatables { get; init; }
			public List<string>? Strings { get; init; }
		}

		[Validatable]
		public partial record Line
		{
			[Required]
			public string? Code { get; init; }
		}

		[Validatable]
		public partial class Generic<T>
		{
			[Required]
			public T? Value { get; set; }
		}
		""";

	[Fact]
	public void CollectionsOfValidatables_Compile()
	{
		var result = GeneratorAssert.CompilesWithoutErrors(Source);
		string generated = GetOrderSource(result);

		Assert.Contains("CombineCollectionItem(collectionItemResult.Result, \"Lines\"", generated);
		Assert.Contains("CombineCollectionItem(collectionItemResult.Result, \"LineArray\"", generated);
		Assert.Contains("CombineCollectionItem(collectionItemResult.Result, \"Immutable\"", generated);
		Assert.Contains("CombineCollectionItem(collectionItemResult.Result, \"Set\"", generated);
		Assert.Contains("CombineCollectionItem(collectionItemResult.Result, \"GenericLines\"", generated);
		Assert.DoesNotContain("\"NotACollectionOfValidatables\"", generated);
		Assert.DoesNotContain("\"Strings\"", generated);
	}

	[Fact]
	public void DisabledByConfiguration_ItemsAreNotValidated()
	{
		var result = GeneratorAssert.CompilesWithoutErrors(
			Source,
			"<ValidateCollectionItems>disable</ValidateCollectionItems>"
		);

		Assert.DoesNotContain("CombineCollectionItem", GetOrderSource(result));
	}

	private static string GetOrderSource(Microsoft.CodeAnalysis.GeneratorRunResult result) =>
		result.GeneratedSources.Single(s => s.HintName.Contains("Order")).SourceText.ToString();
}
