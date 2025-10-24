using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if a value is a valid email address format.
/// This validator ensures that the input contains exactly one '@' character, with characters on both sides.
/// Typically used to validate that a property contains a properly formatted email address.
/// </summary>
[Validator]
[ValidatorDescription("must be a valid email address")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class EmailAddressAttribute : Attribute
{
	private static readonly ValidationMessage ValidationMessage = ValidationMessagesHelper.CreateMessage(
		nameof(EmailAddressAttribute),
		"Must be a valid email address."
	);

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(string? value)
	{
		if (value is null)
		{
			return null;
		}

		// only return true if there is only 1 '@' character
		// and it is neither the first nor the last character
		int index = value.IndexOf('@');

		if (index > 0 && index != value.Length - 1 && index == value.LastIndexOf('@'))
		{
			return null;
		}

		return ValidationMessage;
	}
}
