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
}
