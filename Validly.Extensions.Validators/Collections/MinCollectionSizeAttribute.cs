using System.Collections;
using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Collections;

/// <summary>
/// Validator that checks if a collection has at least a specified minimum size.
/// </summary>
[Validator]
[AttributeUsage(AttributeTargets.Property)]
public sealed class MinCollectionSizeAttribute : Attribute
{
	private readonly int _minSize;
	private readonly ValidationMessage _message;

	/// <param name="minSize">The minimum required number of items.</param>
	[ValidatorDescription("must contain at least {0} items")]
	public MinCollectionSizeAttribute(int minSize)
	{
		_minSize = minSize;
		_message = new ValidationMessage(
			"Must contain at least {0} items.",
			"Validly.Validations.MinCollectionSize",
			_minSize
		);
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid<T>(ICollection<T>? value)
	{
		if (value != null && value.Count < _minSize)
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
	public ValidationMessage? IsValid<T>(IEnumerable<T>? value)
	{
		if (value is null)
		{
			return null;
		}

		if (value is ICollection<T> collection)
		{
			return IsValid(collection);
		}

		return value.Count() < _minSize ? _message : null;
	}
}
