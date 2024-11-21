using System.Runtime.CompilerServices;
using Valigator.Validators;

namespace Valigator.Extensions.Validators.Numbers;

/// <summary>
/// Validator that checks if a numeric value is within a specified range (inclusive)
/// </summary>
[Validator]
[ValidatorDescription("must be between {0} and {1}")]
[AttributeUsage(AttributeTargets.Property)]
public class BetweenAttribute : Attribute
{
	private readonly decimal _min;
	private readonly decimal _max;
	private readonly ValidationMessage _message;

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public BetweenAttribute(decimal min, decimal max)
	{
		_min = min;
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public BetweenAttribute(int min, int max)
	{
		_min = min;
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	public BetweenAttribute(double min, double max)
	{
		_min = (decimal)min;
		_max = (decimal)max;
		_message = CreateMessage();
	}

	private ValidationMessage CreateMessage() =>
		new(
			"Must be between {0} and {1}.",
			ValidationMessagesHelper.GenerateResourceKey(nameof(BetweenAttribute)),
			_min,
			_max
		);

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(decimal? value)
	{
		if (value is not null && (value < _min || value > _max))
		{
			return _message;
		}

		return null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(int? value)
	{
		if (value is null)
		{
			return null;
		}

		if (value < _min || value > _max)
		{
			return _message;
		}

		return null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(object? value)
	{
		if (value is null)
		{
			return null;
		}

		if ((decimal?)value < _min || (decimal?)value > _max)
		{
			return _message;
		}

		return null;
	}
}
