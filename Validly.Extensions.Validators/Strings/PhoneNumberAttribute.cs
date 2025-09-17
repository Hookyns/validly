using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validates that a string is a properly formatted global phone number.
/// </summary>
/// <remarks>
/// The value must:
/// - Start with '+'
/// - Have a country code starting with 1–9
/// - Contain only digits after '+'
/// - Be 8 to 16 characters long (E.164 compliant)
/// 
/// Does not validate actual country codes or national numbering rules.
/// Null values are considered valid.
/// </remarks>
[Validator]
[ValidatorDescription("must be a valid phone number")]
[AttributeUsage(AttributeTargets.Property)]
public class PhoneNumberAttribute : Attribute
{
	private const int MinLength = 8;
	private const int MaxLength = 16;

	private static readonly ValidationMessage ValidationMessage = ValidationMessagesHelper.CreateMessage(
		nameof(PhoneNumberAttribute),
		"Must be a valid phone number."
	);

	/// <summary>
	/// Validates the phone number format.
	/// </summary>
	/// <param name="value">The value to validate. Null is allowed.</param>
	/// <returns>Error message if invalid; null otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(string? value)
	{
		if (value is null)
		{
			return null;
		}

		if (value.Length < MinLength || value.Length > MaxLength)
		{
			return ValidationMessage;
		}

		if (value[0] != '+')
		{
			return ValidationMessage;
		}

		var firstDigit = value[1];

		if (firstDigit < '1' || firstDigit > '9')
		{
			return ValidationMessage;
		}

		for (int i = 2; i < value.Length; i++)
		{
			if (!char.IsDigit(value[i]))
			{
				return ValidationMessage;
			}
		}

		return null;
	}
}