using System.Collections;
using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Collections;

/// <summary>
/// Validator that checks if a collection does not exceed a specified maximum size.
/// </summary>
[Validator]
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxCollectionSizeAttribute : Attribute
{
	private readonly int _maxSize;
	private readonly ValidationMessage _message;

	/// <param name="maxSize">The maximum allowed number of items.</param>
	[ValidatorDescription("must contain no more than {0} items")]
	public MaxCollectionSizeAttribute(int maxSize)
	{
		_maxSize = maxSize;
		_message = new ValidationMessage(
			"Must contain no more than {0} items.",
			"Validly.Validations.MaxCollectionSize",
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
		if (value != null && value.Count > _maxSize)
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

		return value.Count() > _maxSize ? _message : null;
	}
}
