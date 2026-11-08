namespace Validly.SourceGenerator.Utils.Mapping;

/// <summary>
/// Properties of attribute
/// </summary>
/// <remarks>
/// Example: [Range(5, 10)] -> new ValidationAttributeProperties("Range", "5, 10")
/// </remarks>
internal record AttributeProperties
{
	public static readonly AttributeProperties Required =
		new()
		{
			QualifiedName = "Validly.Validators.RequiredAttribute",
			Arguments = new EquatableArray<string>(new[] { "allowEmptyStrings: true" }),
		};

	public static readonly AttributeProperties InEnum =
		new()
		{
			QualifiedName = "Validly.Validators.InEnumAttribute",
			Arguments = EquatableArray<string>.Empty,
		};

	/// <summary></summary>
	public required string QualifiedName { get; init; }

	/// <summary>
	/// Arguments passed to the attribute
	/// </summary>
	public required EquatableArray<string> Arguments { get; init; }
}
