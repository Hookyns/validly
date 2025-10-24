using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if the length of a string is at least a specified minimum length.
/// </summary>
[Validator]
[AttributeUsage(AttributeTargets.Property)]
public sealed class MinLengthAttribute : Attribute
{
	private readonly int _minLength;
	private readonly ValidationMessage _message;

	/// <param name="minLength"></param>
	[ValidatorDescription("must be at least {0} characters long")]
	public MinLengthAttribute(int minLength)
	{
		_minLength = minLength;
		_message = ValidationMessagesHelper.CreateMessage(
			nameof(MinLengthAttribute),
			"Must be at least {0} characters long.",
			minLength
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
		if (value is not null && value.Length < _minLength)
		{
			return _message;
		}

		return null;
	}
}
