using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Numbers;

/// <summary>
/// Validator that checks if a value is greater than or equal to a specified minimum
/// </summary>
[Validator]
[ValidatorDescription("must be greater than or equal to {0}")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class GreaterThanOrEqualAttribute : Attribute
{
	private readonly decimal _min;
	private readonly ValidationMessage _message;

	/// <param name="min">The minimum value.</param>
	public GreaterThanOrEqualAttribute(decimal min)
	{
		_min = min;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum value.</param>
	public GreaterThanOrEqualAttribute(int min)
	{
		_min = min;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum value.</param>
	public GreaterThanOrEqualAttribute(double min)
	{
		_min = (decimal)min;
		_message = CreateMessage();
	}

	private ValidationMessage CreateMessage()
	{
		return new ValidationMessage(
			"Must be greater than or equal to {0}.",
			ValidationMessagesHelper.GenerateResourceKey(nameof(GreaterThanOrEqualAttribute)),
			_min
		);
	}

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

		if (decimalValue < _min)
		{
			return _message;
		}

		return null;
	}
}
