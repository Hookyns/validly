using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Validly.Validators;

/// <summary>
/// Validator that checks if a string matches a specified regular expression pattern.
/// </summary>
[Validator]
[ValidatorDescription("must match the required format")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class MatchAttribute : Attribute
{
	private readonly Regex _regex;

	private readonly ValidationMessage _message;

	/// <param name="pattern">The regular expression pattern to match.</param>
	public MatchAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
	{
		_regex = new Regex(pattern, RegexOptions.Compiled);
		_message = ValidationMessagesHelper.CreateMessage(
			ValidationCodes.Match,
			LegacyValidationCodes.Match,
			[ValidationMessagesHelper.Arg("pattern", pattern)],
			"Must match the required format."
		);
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(string? value)
	{
		if (value is not null && !_regex.IsMatch(value))
		{
			return _message;
		}

		return null;
	}
}
