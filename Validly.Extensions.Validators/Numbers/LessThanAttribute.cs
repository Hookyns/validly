using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Numbers;

/// <summary>
/// Validator that checks if a value is less than a specified maximum
/// </summary>
[Validator]
[ValidatorDescription("must be less than {0}")]
[AttributeUsage(AttributeTargets.Property)]
public class LessThanAttribute : Attribute
{
	private readonly decimal _max;
	private readonly ValidationMessage _message;

	/// <param name="max">The maximum value.</param>
	public LessThanAttribute(decimal max)
	{
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="max">The maximum value.</param>
	public LessThanAttribute(int max)
	{
		_max = max;
		_message = CreateMessage();
	}

	/// <param name="max">The maximum value.</param>
	public LessThanAttribute(double max)
	{
		_max = (decimal)max;
		_message = CreateMessage();
	}

	private ValidationMessage CreateMessage() =>
		new("Must be less than {0}.", ValidationMessagesHelper.GenerateResourceKey(nameof(LessThanAttribute)), _max);

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

		if (decimalValue >= _max)
		{
			return _message;
		}

		return null;
	}
}
