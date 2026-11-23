using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Validly.Extensions.Validators.Collections;
using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Numbers;
using Validly.Naming;

namespace Validly.Tests;

// GitHub #32
[Validatable]
public partial class CollectionOrder
{
	[Required]
	[MinCollectionSize(1)]
	public List<CollectionOrderLine>? Lines { get; set; }
}

[Validatable]
public partial class CollectionOrderLine
{
	[Required]
	public string? ProductCode { get; set; }

	[Between(1, 999)]
	public int Quantity { get; set; }

	private IEnumerable<ValidationMessage> AfterValidate()
	{
		if (ProductCode == "DISCONTINUED")
		{
			yield return new ValidationMessage("Product is discontinued", "OrderLine.Discontinued");
		}
	}
}

[Validatable]
public partial class CollectionKinds
{
	public CollectionOrderLine[]? Array { get; set; }

	public IReadOnlyList<CollectionOrderLine>? ReadOnlyList { get; set; }

	public IEnumerable<CollectionOrderLine>? Enumerable { get; set; }

	public Collection<CollectionOrderLine>? Collection { get; set; }
}

[Validatable]
public partial class CollectionWithSkippedItems
{
	[SkipItemsValidation]
	public List<CollectionOrderLine>? Lines { get; set; }
}

[Validatable]
public partial class CollectionJsonNamed
{
	[JsonPropertyName("order_lines")]
	public List<CollectionJsonNamedLine>? Lines { get; set; }

	public List<CollectionJsonNamedLine>? OtherLines { get; set; }
}

[Validatable]
public partial class CollectionJsonNamedLine
{
	[Required]
	public string? ProductCode { get; set; }

	[Required]
	[JsonPropertyName("explicit_code")]
	public string? ExplicitCode { get; set; }
}

[Validatable]
public partial class CollectionNestedRoot
{
	public CollectionOrder? Order { get; set; }
}

[Validatable]
public partial class CollectionInheritedLine : CollectionOrderLine
{
	[Required]
	public string? Note { get; set; }
}

public class CollectionItemsValidationTests
{
	private static CollectionOrder CreateIssueOrder() =>
		new()
		{
			Lines =
			[
				new CollectionOrderLine { ProductCode = "A1", Quantity = 1 },
				new CollectionOrderLine { ProductCode = null, Quantity = 0 },
				new CollectionOrderLine { ProductCode = "DISCONTINUED", Quantity = 1 },
			],
		};

	[Fact]
	public async Task IssueExample_ItemsAreValidated_WithIndexInPath()
	{
		using var result = await CreateIssueOrder().ValidateAsync();

		Assert.False(result.IsSuccess);

		var failed = result
			.Properties.Where(p => !p.IsSuccess)
			.ToDictionary(p => p.PropertyPath, p => p.Messages.Select(m => m.ResourceKey).ToArray());

		Assert.Equal(["Lines/1/ProductCode", "Lines/1/Quantity", "Lines/2"], failed.Keys);
		Assert.Equal(["OrderLine.Discontinued"], failed["Lines/2"]);
		Assert.Empty(result.Global);
	}

	[Fact]
	public async Task ValidItems_AreNotReported()
	{
		var order = new CollectionOrder
		{
			Lines = [new CollectionOrderLine { ProductCode = "A1", Quantity = 1 }, new CollectionOrderLine { ProductCode = "A2", Quantity = 2 }],
		};

		using var result = await order.ValidateAsync();

		Assert.True(result.IsSuccess);
		Assert.Equal(["Lines"], result.Properties.Select(p => p.PropertyPath));
	}

	[Fact]
	public async Task NullCollectionAndNullItems_AreSkipped()
	{
		using (var nullCollection = await new CollectionOrder { Lines = null }.ValidateAsync())
		{
			// Only the Required rule of the collection itself fails
			Assert.Equal("Lines", Assert.Single(nullCollection.Properties, p => !p.IsSuccess).PropertyPath);
		}

		using var nullItem = await new CollectionOrder
		{
			Lines = [null!, new CollectionOrderLine { ProductCode = null, Quantity = 1 }],
		}.ValidateAsync();

		Assert.Equal("Lines/1/ProductCode", Assert.Single(nullItem.Properties, p => !p.IsSuccess).PropertyPath);
	}

	[Fact]
	public async Task AllCollectionKinds_AreValidated()
	{
		var invalid = new CollectionOrderLine { ProductCode = null, Quantity = 1 };
		var valid = new CollectionOrderLine { ProductCode = "A", Quantity = 1 };

		using var result = await new CollectionKinds
		{
			Array = [valid, invalid],
			ReadOnlyList = [invalid],
			Enumerable = YieldLines(valid, valid, invalid),
			Collection = [invalid],
		}.ValidateAsync();

		Assert.Equal(
			["Array/1/ProductCode", "ReadOnlyList/0/ProductCode", "Enumerable/2/ProductCode", "Collection/0/ProductCode"],
			result.Properties.Where(p => !p.IsSuccess).Select(p => p.PropertyPath)
		);

		static IEnumerable<CollectionOrderLine> YieldLines(params CollectionOrderLine[] lines)
		{
			foreach (var line in lines)
			{
				yield return line;
			}
		}
	}

	[Fact]
	public async Task SkipItemsValidation_ItemsAreNotValidated()
	{
		using var result = await new CollectionWithSkippedItems
		{
			Lines = [new CollectionOrderLine { ProductCode = null, Quantity = 0 }],
		}.ValidateAsync();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task JsonPointer_ContainsIndex_NamingPolicyIsNotAppliedToIt()
	{
		using var result = await new CollectionJsonNamed
		{
			Lines = [new CollectionJsonNamedLine { ProductCode = "A", ExplicitCode = "B" }, new CollectionJsonNamedLine()],
			OtherLines = [new CollectionJsonNamedLine { ProductCode = "A", ExplicitCode = "B" }, new CollectionJsonNamedLine { ProductCode = "A", ExplicitCode = "B" }, new CollectionJsonNamedLine { ExplicitCode = "B" }],
		}.ValidateAsync();

		var pointers = result
			.Properties.Where(p => !p.IsSuccess)
			.Select(p => p.GetJsonPointerPath(PropertyNamingPolicy.SnakeCaseUpper))
			.ToArray();

		Assert.Equal(["order_lines/1/PRODUCT_CODE", "order_lines/1/explicit_code", "OTHER_LINES/2/PRODUCT_CODE"], pointers);

		using var problemDetails = JsonDocument.Parse(result.GetProblemDetailsJson(PropertyNamingPolicy.CamelCase));
		var problemPointers = problemDetails
			.RootElement.GetProperty("errors")
			.EnumerateArray()
			.Select(e => e.GetProperty("pointer").GetString())
			.ToArray();

		Assert.Equal(["#/order_lines/1/productCode", "#/order_lines/1/explicit_code", "#/otherLines/2/productCode"], problemPointers);
	}

	[Fact]
	public async Task CollectionInNestedObject_PathContainsAllSegments()
	{
		using var result = await new CollectionNestedRoot { Order = CreateIssueOrder() }.ValidateAsync();

		Assert.Equal(
			["Order/Lines/1/ProductCode", "Order/Lines/1/Quantity", "Order/Lines/2"],
			result.Properties.Where(p => !p.IsSuccess).Select(p => p.PropertyPath)
		);

		Assert.Equal(
			["order/lines/1/productCode", "order/lines/1/quantity", "order/lines/2"],
			result.Properties.Where(p => !p.IsSuccess).Select(p => p.GetJsonPointerPath(PropertyNamingPolicy.CamelCase))
		);
	}

	[Fact]
	public async Task DerivedItem_IsValidatedPolymorphically()
	{
		var order = new CollectionOrder
		{
			Lines = [new CollectionInheritedLine { ProductCode = "A", Quantity = 1, Note = null }],
		};

		using var result = await order.ValidateAsync();

		Assert.Equal("Lines/0/Note", Assert.Single(result.Properties, p => !p.IsSuccess).PropertyPath);
	}

	[Fact]
	public async Task LargeCollection_IndexesAboveCache()
	{
		var lines = Enumerable.Range(0, 300).Select(i => new CollectionOrderLine { ProductCode = "A", Quantity = 1 }).ToList();
		lines[299].Quantity = 0;

		using var result = await new CollectionOrder { Lines = lines }.ValidateAsync();

		Assert.Equal("Lines/299/Quantity", Assert.Single(result.Properties, p => !p.IsSuccess).PropertyPath);
	}

	[Fact]
	public async Task ProblemDetails_ItemMessageHasItemPointer()
	{
		using var result = await CreateIssueOrder().ValidateAsync();

		var errors = result.GetProblemDetails().Errors;

		Assert.Contains(errors, e => e.Pointer == "#/Lines/2" && e.ResourceKey == "OrderLine.Discontinued");
	}
}

[Validatable]
public partial class CollectionEdgeCases
{
	// Static and indexer properties are not validated as collections
	public static List<CollectionOrderLine> Templates { get; } = [new CollectionOrderLine()];

	public System.Collections.Immutable.ImmutableArray<CollectionOrderLine> Immutable { get; set; }
}

public class CollectionItemsEdgeCaseTests
{
	[Fact]
	public void DefaultImmutableArray_IsSkipped_StaticCollectionIsIgnored()
	{
		using var result = new CollectionEdgeCases().Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void ImmutableArray_ItemsAreValidated()
	{
		using var result = new CollectionEdgeCases
		{
			Immutable = [new CollectionOrderLine { ProductCode = "A", Quantity = 1 }, new CollectionOrderLine { Quantity = 1 }],
		}.Validate();

		Assert.Equal("Immutable/1/ProductCode", Assert.Single(result.Properties, p => !p.IsSuccess).PropertyPath);
	}

	[Fact]
	public void ObjectWithCollection_HasSynchronousValidate()
	{
		using var result = new CollectionOrder { Lines = [new CollectionOrderLine { Quantity = 1 }] }.Validate();

		Assert.Equal("Lines/0/ProductCode", Assert.Single(result.Properties, p => !p.IsSuccess).PropertyPath);
	}

	[Fact]
	public void LargeNestedCollection_IsCombinedWithoutLookups()
	{
		var lines = Enumerable.Range(0, 5000).Select(_ => new CollectionOrderLine { Quantity = 0 }).ToList();

		using var result = new CollectionNestedRoot { Order = new CollectionOrder { Lines = lines } }.Validate();

		Assert.Equal(10_000, result.Properties.Count(p => !p.IsSuccess));
		Assert.Contains(result.Properties, p => p.PropertyPath == "Order/Lines/4999/Quantity");
	}
}
