using System.Runtime.CompilerServices;

namespace Validly.Validators;

/// <summary>
/// Validator that checks if a numeric value is within a specified range (inclusive)
/// </summary>
[Validator]
[ValidatorDescription("must be between {0} and {1}")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class BetweenAttribute : Attribute
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
		ValidationMessagesHelper.CreateMessage(ValidationCodes.Between, LegacyValidationCodes.Between, [ValidationMessagesHelper.Arg("min", _min), ValidationMessagesHelper.Arg("max", _max)], "Must be between {0} and {1}.",
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
		return value is not null && (value < _min || value > _max) ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(decimal value)
	{
		return value < _min || value > _max ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(int? value)
	{
		return value is not null && (value < _min || value > _max) ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(int value)
	{
		return value < _min || value > _max ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(byte? value)
	{
		return value is not null && (value < _min || value > _max) ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(byte value)
	{
		return value < _min || value > _max ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(long? value)
	{
		return value is not null && (value < _min || value > _max) ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(long value)
	{
		return value < _min || value > _max ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(short? value)
	{
		return value is not null && (value < _min || value > _max) ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(short value)
	{
		return value < _min || value > _max ? _message : null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid(object? value)
	{
		return value is not null && ((decimal?)value < _min || (decimal?)value > _max) ? _message : null;
	}
}
