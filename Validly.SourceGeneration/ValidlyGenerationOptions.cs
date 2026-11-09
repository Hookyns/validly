using Microsoft.CodeAnalysis;

namespace Validly.SourceGeneration;

/// <summary>
/// Options of <see cref="ValidlySourceGeneration.Generate"/>
/// </summary>
public sealed record ValidlyGenerationOptions
{
	/// <summary>
	/// Name of the target type (without type parameters); null = the source type itself
	/// </summary>
	public string? TypeName { get; init; }

	/// <summary>
	/// Namespace of the target type; null = namespace of the source type, empty string = global namespace
	/// </summary>
	public string? Namespace { get; init; }

	/// <summary>
	/// The target type is a record (true) or a class (false); null = same as the source type
	/// </summary>
	public bool? IsRecord { get; init; }

	/// <summary>
	/// Accessibility of the target type; null = same as the source type
	/// </summary>
	public Accessibility? Accessibility { get; init; }

	/// <summary>
	/// The target type is sealed (smaller and faster generated code); used when <see cref="TypeName"/> is set
	/// </summary>
	public bool IsSealed { get; init; }

	/// <summary>
	/// Maps the name of a source property to the name of the target property; null = the property is excluded.
	/// Null map = all properties with the same names.
	/// </summary>
	public Func<string, string?>? PropertyMap { get; init; }

	/// <summary>
	/// Properties (rules) of the ancestors of the source type are generated as properties of the target type (default)
	/// </summary>
	public bool IncludeBaseProperties { get; init; } = true;

	/// <summary>
	/// BeforeValidate/AfterValidate hooks are called; the target type must have them (default: not copied)
	/// </summary>
	public bool CopyHooks { get; init; }

	/// <summary>
	/// [CustomValidation] rules are kept; the target type must implement the generated custom validation
	/// interface (default: not copied)
	/// </summary>
	public bool CopyCustomValidation { get; init; }

	/// <summary>
	/// Nested validatable objects and items of collections of validatable objects are validated (default).
	/// The types of the target's properties must be validatable then.
	/// </summary>
	public bool ValidateNestedObjects { get; init; } = true;

	/// <summary>
	/// Required validator is added to non-nullable properties (default)
	/// </summary>
	public bool AutoRequired { get; init; } = true;

	/// <summary>
	/// InEnum validator is added to enum properties (default)
	/// </summary>
	public bool AutoInEnum { get; init; } = true;

	/// <summary>
	/// Validation exits on the first failure
	/// </summary>
	public bool ExitEarly { get; init; }

	/// <summary>
	/// Items of collections of validatable objects are validated (default)
	/// </summary>
	public bool ValidateCollectionItems { get; init; } = true;

	/// <summary>
	/// JSON naming policy applied at compile time: camelCase, snake_case_lower, snake_case_upper,
	/// kebab_case_lower, kebab_case_upper or none; null = resolved at runtime
	/// </summary>
	public string? JsonNamingPolicy { get; init; }
}
