using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Common;

/// <summary>
/// Validator that ensures a value is present, enforcing it as required
/// </summary>
[Validator]
[ValidatorDescription("is required")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredAttribute : Attribute
{
	private static readonly ValidationMessage RequiredMessage = new(
		"A value is required.",
		"Validly.Validations.Required"
	);

	private readonly bool _allowEmptyStrings;

	/// <summary>
	/// Validator that ensures a value is present, enforcing it as required.
	/// If the value is a string, it can optionally allow empty strings based on the configuration of <paramref name="allowEmptyStrings"/>.
	/// Typically used to validate that a property has a non-null or non-empty value.
	/// </summary>
	/// <param name="allowEmptyStrings">If true, empty strings are considered valid values; otherwise, they are treated as missing values.</param>
	public RequiredAttribute(bool allowEmptyStrings = false)
	{
		_allowEmptyStrings = allowEmptyStrings;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(string? value)
	{
		if (value is null || (!_allowEmptyStrings && value == string.Empty))
		{
			return RequiredMessage;
		}

		return null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public ValidationMessage? IsValid<T>(T? value)
	{
		return value is null ? RequiredMessage : null;
	}
}
