using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if a value is a valid password format.
/// This validator ensures the input meets configurable complexity requirements,
/// such as minimum length, presence of uppercase letters, digits, and special characters.
/// Typically used to validate that a property contains a secure password.
/// </summary>
[Validator]
[ValidatorDescription("must be a valid password")]
[AttributeUsage(AttributeTargets.Property)]
public class PasswordAttribute : Attribute
{
	private readonly int _minLength;
	private readonly bool _requireSpecialChar;
	private readonly bool _requireUpperCase;
	private readonly bool _requireDigit;

	private static readonly ValidationMessage ValidationMessage = ValidationMessagesHelper.CreateMessage(
		nameof(PasswordAttribute),
		"Must be a valid password."
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="PasswordAttribute"/> class.
	/// </summary>
	/// <param name="minLength">The minimum length of the password. Default is 7.</param>
	/// <param name="requireSpecialChar">
	/// Whether the password must contain at least one special character (non-alphanumeric).
	/// Default is <c>false</c>.
	/// </param>
	/// <param name="requireUpperCase">
	/// Whether the password must contain at least one uppercase ASCII letter (A–Z).
	/// Default is <c>false</c>.
	/// </param>
	/// <param name="requireDigit">
	/// Whether the password must contain at least one digit (0–9).
	/// Default is <c>false</c>.
	/// </param>
	public PasswordAttribute(
		int minLength = 7,
		bool requireSpecialChar = false,
		bool requireUpperCase = false,
		bool requireDigit = false)
	{
		_minLength = minLength;
		_requireSpecialChar = requireSpecialChar;
		_requireUpperCase = requireUpperCase;
		_requireDigit = requireDigit;
	}

	/// <summary>
	/// Validates the specified value as a password.
	/// </summary>
	/// <param name="value">The string value to validate. Can be <c>null</c> or empty.</param>
	/// <returns>
	/// A <see cref="ValidationMessage"/> if validation fails; otherwise, <c>null</c>.
	/// Validation fails if any of the following conditions are met:
	/// - the value is <c>null</c> or shorter than <see cref="_minLength"/>;
	/// - <see cref="_requireSpecialChar"/> is <c>true</c> and no special character (non-alphanumeric) is found;
	/// - <see cref="_requireUpperCase"/> is <c>true</c> and no uppercase letter (A–Z) is present;
	/// - <see cref="_requireDigit"/> is <c>true</c> and no digit (0–9) is present.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(string? value)
	{
		if (value is null || value.Length < _minLength)
		{
			return ValidationMessage;
		}

		bool hasSpecialChar = false;
		bool hasUpperCase = false;
		bool hasDigit = false;

		foreach (char c in value)
		{
			if (_requireSpecialChar && !hasSpecialChar && !char.IsLetterOrDigit(c))
			{
				hasSpecialChar = true;
			}

			if (_requireUpperCase && !hasUpperCase && char.IsUpper(c))
			{
				hasUpperCase = true;
			}

			if (_requireDigit && !hasDigit && char.IsDigit(c))
			{
				hasDigit = true;
			}

			if (hasSpecialChar && hasUpperCase && hasDigit)
			{
				break;
			}
		}

		if ((_requireSpecialChar && !hasSpecialChar)
			|| (_requireUpperCase && !hasUpperCase)
			|| (_requireDigit && !hasDigit))
		{
			return ValidationMessage;
		}

		return null;
	}
}