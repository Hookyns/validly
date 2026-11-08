namespace Validly.Details;

/// <summary>
/// Details of validation error
/// </summary>
public class ValidationErrorDetail
{
	/// <summary>
	/// Human-readable explanation
	/// </summary>
	public required string Detail { get; init; }

	/// <summary>
	/// Stable, public code of the error; usable as a key to translate the <see cref="Detail"/>
	/// </summary>
	public required string Code { get; init; }

	/// <summary>
	/// Resource key for the <see cref="Detail"/> so it can be translated
	/// </summary>
	[Obsolete("Use Code. ResourceKey is an alias of Code.")]
	public string ResourceKey => Code;

	/// <summary>
	/// Positional arguments used in the <see cref="Detail"/>
	/// </summary>
	public required object?[] Args { get; init; }

	/// <summary>
	/// Named arguments, e.g. ("min", 5), ("max", 20); null when the message has none
	/// </summary>
	public IReadOnlyList<KeyValuePair<string, object?>>? NamedArgs { get; init; }

	/// <summary>
	/// Indicates a location of the data causing the validation error
	/// </summary>
	/// <remarks>
	/// JSON pointer - https://www.rfc-editor.org/rfc/rfc9457.html#RFC6901
	/// </remarks>
	public required string Pointer { get; init; }

	/// <summary>
	/// Name of the field
	/// </summary>
	public required string? FieldName { get; init; }
}
