using System.Diagnostics.CodeAnalysis;

namespace Validly.Validators;

internal static class ValidationMessagesHelper
{
	public static string GenerateResourceKey(string validatorAttributeName) =>
		$"Validly.Validations.{validatorAttributeName.Replace("Attribute", "")}.Error";

	public static ValidationMessage CreateMessage(
		string validatorAttributeName,
		[StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
		params object?[] args
	) => new(message, GenerateResourceKey(validatorAttributeName), args);
}
