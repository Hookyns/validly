using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Strings;

/// <summary>
/// Validator that checks if a string length is within a specified range (inclusive)
/// </summary>
[Validator]
[ValidatorDescription("must be between {0} and {1}")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class LengthBetweenAttribute : Attribute
{
	private readonly int _min;
	private readonly int _max;
	private readonly ValidationMessage _message;

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public LengthBetweenAttribute(int min, int max)
	{
		_min = min;
		_max = max;
		_message = ValidationMessagesHelper.CreateMessage(
			nameof(LengthBetweenAttribute),
			"Length must be between {0} and {1}.",
			_min,
			_max
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
		if (value is not null && (value.Length < _min || value.Length > _max))
		{
			return _message;
		}

		return null;
	}
}
