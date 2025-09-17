using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if a value is a strong password.
/// This validator enforces strict complexity requirements by requiring:
/// - minimum length,
/// - at least one uppercase letter (A–Z),
/// - at least one digit (0–9),
/// - and at least one special character (non-alphanumeric).
/// </summary>
[Validator]
[ValidatorDescription("must be a strong password")]
[AttributeUsage(AttributeTargets.Property)]
public class StrongPasswordAttribute : PasswordAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="StrongPasswordAttribute"/> class 
	/// with specified minimum length and predefined complexity rules.
	/// </summary>
	/// <param name="minLength">The minimum length required for the password. Default is 7.</param>
	/// <remarks>
	/// This constructor automatically enables checks for:
	/// - uppercase letters,
	/// - digits,
	/// - special characters.
	/// These cannot be disabled and are always enforced.
	/// </remarks>
	public StrongPasswordAttribute(int minLength = 7)
		: base(minLength, requireSpecialChar: true, requireUpperCase: true, requireDigit: true)
	{
	}
}