using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Numbers;

/// <summary>
/// Validator that checks if a value is less than or equal to a specified maximum.
/// </summary>
[Validator]
[ValidatorDescription("must be less than or equal to {0}")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class LessThanOrEqualAttribute : Attribute
{
	private readonly decimal _max;
	private readonly ValidationMessage _message;

	/// <param name="max">The maximum value.</param>
	public LessThanOrEqualAttribute(decimal max)
	{
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="max">The maximum value.</param>
	public LessThanOrEqualAttribute(int max)
	{
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="max">The maximum value.</param>
	public LessThanOrEqualAttribute(double max)
	{
		_max = (decimal)max;
		_message = CreateMessage();
	}

	private ValidationMessage CreateMessage() =>
		new(
			"Must be less than or equal to {0}.",
			ValidationMessagesHelper.GenerateResourceKey(nameof(LessThanOrEqualAttribute)),
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
		if (value is null)
		{
			return null;
		}

		decimal decimalValue = Convert.ToDecimal(value);

		if (decimalValue > _max)
		{
			return _message;
		}

		return null;
	}
}
