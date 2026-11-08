using System.Diagnostics.CodeAnalysis;

namespace Validly.Validators;

internal static class ValidationMessagesHelper
{
	/// <summary>
	/// Code of the built-in validator; the legacy one when the legacy codes are enabled (never mixed)
	/// </summary>
	public static string Code(string code, string legacyCode) =>
		ValidlyOptions.UseLegacyValidationCodes ? legacyCode : code;

	/// <summary>
	/// Create the message of a built-in validator; built-in validators create their messages once (per rule)
	/// </summary>
	public static ValidationMessage CreateMessage(
		string code,
		string legacyCode,
		KeyValuePair<string, object?>[]? namedArgs,
		[StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
		params object?[] args
	) => new(message, Code(code, legacyCode), args) { NamedArgs = namedArgs };

	public static KeyValuePair<string, object?> Arg(string name, object? value) => new(name, value);
}
