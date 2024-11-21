using System.Runtime.CompilerServices;
using Valigator.Validators;

namespace Valigator.Extensions.Validators.Numbers;

/// <summary>
/// Validator that checks if a value is greater than a specified minimum
/// </summary>
[Validator]
[ValidatorDescription("must be greater than {0}")]
[AttributeUsage(AttributeTargets.Property)]
public class GreaterThanAttribute : Attribute
{
	private readonly decimal _min;
	private readonly ValidationMessage _message;

	/// <param name="min">The minimum value.</param>
	public GreaterThanAttribute(decimal min)
	{
		_min = min;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum value.</param>
	public GreaterThanAttribute(int min)
	{
		_min = min;
		_message = CreateMessage();
	}

	/// <param name="min">The minimum value.</param>
	public GreaterThanAttribute(double min)
	{
		_min = (decimal)min;
		_message = CreateMessage();
	}

	private ValidationMessage CreateMessage() =>
		new(
			"Must be greater than {0}.",
			ValidationMessagesHelper.GenerateResourceKey(nameof(GreaterThanAttribute)),
			_min
		);

	// /// <param name="expression">Expression used to access a property with the value.</param>
	// public GreaterThanValidator([AsExpression] string expression)
	// {
	// 	throw new NotImplementedException();
	// }

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

		if (decimalValue <= _min)
		{
			return _message;
		}

		return null;
	}
}
