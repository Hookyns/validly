using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Numbers;

/// <summary>
/// Validator that checks if a numeric value is within a specified range (exclusive)
/// </summary>
[Validator]
[ValidatorDescription("must be between {0} and {1}")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class ExclusiveBetweenAttribute : Attribute
{
	private readonly decimal _min;
	private readonly decimal _max;
	private readonly ValidationMessage _message;

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public ExclusiveBetweenAttribute(decimal min, decimal max)
	{
		_min = min;
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public ExclusiveBetweenAttribute(int min, int max)
	{
		_min = min;
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public ExclusiveBetweenAttribute(double min, double max)
	{
		_min = (decimal)min;
		_max = (decimal)max;
		_message = CreateMessage();
	}

	private ValidationMessage CreateMessage() =>
		new(
			"Must be between {0} and {1} (exclusive).",
			ValidationMessagesHelper.GenerateResourceKey(nameof(ExclusiveBetweenAttribute)),
			_min,
			_max
		);

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(object? value) // TODO: Add overloads to prevent conversion
	{
		if (value is not null && ((decimal)value <= _min || (decimal)value >= _max))
		{
			return _message;
		}

		return null;
	}
}
