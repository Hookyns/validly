namespace Validly.Naming;

/// <summary>
/// Built-in naming policies; they match the policies of <c>System.Text.Json.JsonNamingPolicy</c>
/// </summary>
/// <remarks>
/// This enum is shared with the source generator, so the names can be converted at compile time.
/// </remarks>
public enum ValidlyNamingPolicy
{
	/// <summary>
	/// Property names are used as they are declared
	/// </summary>
	None = 0,

	/// <summary>
	/// camelCase
	/// </summary>
	CamelCase,

	/// <summary>
	/// snake_case
	/// </summary>
	SnakeCaseLower,

	/// <summary>
	/// SNAKE_CASE
	/// </summary>
	SnakeCaseUpper,

	/// <summary>
	/// kebab-case
	/// </summary>
	KebabCaseLower,

	/// <summary>
	/// KEBAB-CASE
	/// </summary>
	KebabCaseUpper,
}
