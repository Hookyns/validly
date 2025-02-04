using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Validly;

/// <summary>
/// Error message generated by the validator
/// </summary>
/// <param name="Message">Translated message, or default message (e.g., in English)</param>
/// <param name="ResourceKey">Resource key for localization, which may contain placeholders (e.g., {0}) for passed arguments.</param>
/// <param name="Args">Arguments</param>
public record ValidationMessage(
	[StringSyntax(StringSyntaxAttribute.CompositeFormat)] string Message,
	string ResourceKey,
	params object?[] Args
)
{
	/// <summary>
	/// Empty validation message
	/// </summary>
	public static ValidationMessage Empty = new(string.Empty, string.Empty);

	/// <summary>
	/// String with comma-separated list of arguments converted to JSON strings
	/// </summary>
	/// <remarks>
	/// Prepared to be used within array brackets like: $"[{ArgsJson}]"
	/// </remarks>
	public string ArgsJson { get; } =
		string.Join(", ", Args.Select(static x => JsonValue.Create(x)?.ToJsonString() ?? "null"));
}
