using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Utils.Mapping;

internal record PropertyProperties
{
	public required string PropertyName { get; init; }

	public required string DisplayName { get; init; }

	/// <summary>
	/// Explicit JSON name of the property from [JsonPropertyName]
	/// </summary>
	public required string? JsonName { get; init; }

	public required string PropertyType { get; init; }

	/// <summary>
	/// Kind of the type
	/// </summary>
	/// <remarks>
	/// May be class, struct, enum, interface, delegate, record,...
	/// </remarks>
	public required TypeKind PropertyTypeKind { get; init; }

	public required bool Nullable { get; init; }

	public required bool PropertyIsOfValidatableType { get; init; }

	public required EquatableArray<AttributeProperties> Attributes { get; init; }

	/// <summary>
	/// Kind of the collection when the property is a collection of validatable objects; None otherwise
	/// </summary>
	public CollectionItemsKind CollectionItemsKind { get; init; }

	/// <summary>
	/// Items of the collection must not be validated ([SkipItemsValidation])
	/// </summary>
	public bool SkipItemsValidation { get; init; }
}

/// <summary>
/// How the items of a collection of validatable objects are enumerated
/// </summary>
internal enum CollectionItemsKind
{
	/// <summary>
	/// Not a collection of validatable objects
	/// </summary>
	None,

	/// <summary>
	/// Array; indexed, Length
	/// </summary>
	Array,

	/// <summary>
	/// List, IList, IReadOnlyList; indexed, Count
	/// </summary>
	List,

	/// <summary>
	/// Any other IEnumerable; enumerated
	/// </summary>
	Enumerable,
}
