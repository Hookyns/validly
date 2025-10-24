using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if the length of a string does not exceed a specified maximum length.
/// </summary>
[Validator]
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxLengthAttribute : Attribute
{
	private readonly int _maxLength;
	private readonly ValidationMessage _message;

	/// <param name="maxLength">The maximum allowed length for the string value.</param>
	[ValidatorDescription("must be no more than {0} characters long")]
	public MaxLengthAttribute(int maxLength)
	{
		_maxLength = maxLength;
		_message = ValidationMessagesHelper.CreateMessage(
			nameof(MaxLengthAttribute),
			"Must be no more than {0} characters long.",
			maxLength
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
		if (value is not null && value.Length > _maxLength)
		{
			return _message;
		}

		return null;
	}
}
