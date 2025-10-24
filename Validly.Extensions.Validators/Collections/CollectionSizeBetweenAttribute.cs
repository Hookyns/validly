using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Collections;

/// <summary>
/// Validator that checks if a collection size is within a specified range (inclusive).
/// </summary>
[Validator]
[AttributeUsage(AttributeTargets.Property)]
public sealed class CollectionSizeBetweenAttribute : Attribute
{
	private readonly int _minSize;
	private readonly int _maxSize;
	private readonly ValidationMessage _message;

	/// <param name="minSize">The minimum number of items allowed.</param>
	/// <param name="maxSize">The maximum number of items allowed.</param>
	[ValidatorDescription("must contain between {0} and {1} items")]
	public CollectionSizeBetweenAttribute(int minSize, int maxSize)
	{
		_minSize = minSize;
		_maxSize = maxSize;
		_message = new ValidationMessage(
			"Must contain between {0} and {1} items.",
			"Validly.Validations.CollectionSizeBetween",
			_minSize,
			_maxSize
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
		if (value is not null && (value.Count < _minSize || value.Count > _maxSize))
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

		int count = value.Count();

		if (count < _minSize || count > _maxSize)
		{
			return _message;
		}

		return null;
	}
}
