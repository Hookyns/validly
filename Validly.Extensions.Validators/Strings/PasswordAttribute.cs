using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if a value is a valid password format.
/// This validator ensures the input meets configurable complexity requirements,
/// such as minimum length, required counts of uppercase letters, digits, and special characters.
/// Typically used to validate that a property contains a secure password.
/// </summary>
[Validator]
[ValidatorDescription("must be a valid password")]
[AttributeUsage(AttributeTargets.Property)]
public class PasswordAttribute : Attribute
{
	private readonly int _minLength;
	private readonly int _numberOfRequiredSpecialCharacters;
	private readonly int _numberOfRequiredUpperCaseCharacters;
	private readonly int _numberOfRequiredDigitCharacters;

	/// <summary>
	/// Initializes a new instance of the <see cref="PasswordAttribute"/> class.
	/// </summary>
	/// <param name="minLength">The minimum length of the password. Must be non-negative. Default is 12.</param>
	/// <param name="numberOfRequiredSpecialCharacters">
	/// The minimum number of special characters (non-alphanumeric) required. Must be non-negative. Default is 0.
	/// </param>
	/// <param name="numberOfRequiredUpperCaseCharacters">
	/// The minimum number of uppercase ASCII letters (A–Z) required. Must be non-negative. Default is 0.
	/// </param>
	/// <param name="numberOfRequiredDigitCharacters">
	/// The minimum number of digit characters (0–9) required. Must be non-negative. Default is 0.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when any of the numeric parameters is negative.
	/// </exception>
	public PasswordAttribute(
		int minLength = 12,
		int numberOfRequiredSpecialCharacters = 0,
		int numberOfRequiredUpperCaseCharacters = 0,
		int numberOfRequiredDigitCharacters = 0)
	{
		if (minLength < 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(minLength),
				minLength,
				"Minimum length cannot be negative.");
		}

		if (numberOfRequiredSpecialCharacters < 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(numberOfRequiredSpecialCharacters),
				numberOfRequiredSpecialCharacters,
				"Number of required special characters cannot be negative.");
		}

		if (numberOfRequiredUpperCaseCharacters < 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(numberOfRequiredUpperCaseCharacters),
				numberOfRequiredUpperCaseCharacters,
				"Number of required uppercase characters cannot be negative.");
		}

		if (numberOfRequiredDigitCharacters < 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(numberOfRequiredDigitCharacters),
				numberOfRequiredDigitCharacters,
				"Number of required digit characters cannot be negative.");
		}

		_minLength = minLength;
		_numberOfRequiredSpecialCharacters = numberOfRequiredSpecialCharacters;
		_numberOfRequiredUpperCaseCharacters = numberOfRequiredUpperCaseCharacters;
		_numberOfRequiredDigitCharacters = numberOfRequiredDigitCharacters;
	}

	/// <summary>
	/// Validates the specified value as a password.
	/// </summary>
	/// <param name="value">The string value to validate. Can be <c>null</c> or empty.</param>
	/// <returns>
	/// A <see cref="ValidationMessage"/> if validation fails; otherwise, <c>null</c>.
	/// Validation fails if any of the following conditions are met:
	/// - the value is <c>null</c> or shorter than <see cref="_minLength"/>;
	/// - the count of special characters is less than <see cref="_numberOfRequiredSpecialCharacters"/>;
	/// - the count of uppercase letters is less than <see cref="_numberOfRequiredUpperCaseCharacters"/>;
	/// - the count of digits is less than <see cref="_numberOfRequiredDigitCharacters"/>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(string? value)
	{
		if (value is null || value.Length < _minLength)
		{
			return CreateValidationMessage($"Password must be at least {_minLength} characters long.");
		}

		int specialCharCount = 0;
		int upperCaseCount = 0;
		int digitCount = 0;

		foreach (char c in value)
		{
			if (_numberOfRequiredSpecialCharacters > 0 && !char.IsLetterOrDigit(c))
			{
				specialCharCount++;
			}

			if (_numberOfRequiredUpperCaseCharacters > 0 && char.IsUpper(c))
			{
				upperCaseCount++;
			}

			if (_numberOfRequiredDigitCharacters > 0 && char.IsDigit(c))
			{
				digitCount++;
			}

			if (specialCharCount >= _numberOfRequiredSpecialCharacters
				&& upperCaseCount >= _numberOfRequiredUpperCaseCharacters
				&& digitCount >= _numberOfRequiredDigitCharacters)
			{
				break;
			}
		}

		if (specialCharCount < _numberOfRequiredSpecialCharacters)
		{
			return CreateValidationMessage($"The password must contain at least {_numberOfRequiredSpecialCharacters} special character(s).");
		}

		if (upperCaseCount < _numberOfRequiredUpperCaseCharacters)
		{
			return CreateValidationMessage($"The password must contain at least {_numberOfRequiredUpperCaseCharacters} uppercase letter(s).");
		}

		if (digitCount < _numberOfRequiredDigitCharacters)
		{
			return CreateValidationMessage($"The password must contain at least {_numberOfRequiredDigitCharacters} digit(s).");
		}

		return null;
	}

	private static ValidationMessage CreateValidationMessage(string message)
	{
		return ValidationMessagesHelper.CreateMessage(nameof(PasswordAttribute), message);
	}
}