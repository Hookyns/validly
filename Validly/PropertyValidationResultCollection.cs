using System.Buffers;
using System.Collections;

namespace Validly;

/// <summary>
/// Collection of <see cref="PropertyValidationResult"/>s for all the properties of the object being validated.
/// </summary>
public sealed class PropertyValidationResultCollection : IReadOnlyList<PropertyValidationResult>, IDisposable
{
	/// <summary>
	/// Empty, readonly, collection
	/// </summary>
	public static readonly PropertyValidationResultCollection Empty = new();

	/// <summary>
	/// Pool for <see cref="PropertyValidationResult"/>s
	/// </summary>
	private static readonly ArrayPool<PropertyValidationResult> PropertyValidationResultPool =
		ArrayPool<PropertyValidationResult>.Shared;

	/// <summary>
	/// Pool for <see cref="ValidationMessage"/>s
	/// </summary>
	private static readonly ArrayPool<ValidationMessage> ValidationMessagePool = ArrayPool<ValidationMessage>.Shared;

	private bool _disposed = true;

	/// <summary>
	/// Array of all properties results
	/// </summary>
	/// <remarks>
	/// It is pooled collection. Real count of the items is stored in <see cref="_count"/>.
	/// Objects inside the array are part of the pool; they are reused and not reallocated.
	/// </remarks>
	// ReSharper disable once UseCollectionExpression
	private PropertyValidationResult[] _propertiesResult = Array.Empty<PropertyValidationResult>();

	/// <summary>
	/// Count of items in <see cref="_propertiesResult"/> pooled array
	/// </summary>
	private int _count;

	/// <summary>
	/// Buffer of messages for all the properties separately stored in <see cref="_propertiesResult"/>
	/// </summary>
	/// <remarks>
	/// Messages are reassigned and not reused; this buffer is pooled so the array itself do not allocate memory.
	/// </remarks>
	// ReSharper disable once UseCollectionExpression
	private ValidationMessage[] _messages = Array.Empty<ValidationMessage>();

	/// <summary>
	/// Size of the message buffer for one property
	/// </summary>
	private int _messagesPerProperty;

	/// <inheritdoc />
	public int Count => _count;

	internal PropertyValidationResult[] ArrayBuffer => _propertiesResult;

	/// <summary>
	/// True if validation of all the properties was successful
	/// </summary>
	// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
	public bool IsSuccess
	{
		get
		{
			for (int propIndex = 0; propIndex < _count; propIndex++)
			{
				if (!_propertiesResult[propIndex].IsSuccess)
				{
					return false;
				}
			}

			return true;
		}
	}

	/// <summary>
	/// Create collection of property results
	/// </summary>
	/// <param name="properties"></param>
	public PropertyValidationResultCollection(PropertyValidationResult[] properties)
	{
		// Users/developer can use this Ctor; this ctor creates instance that should work without pooling
		_propertiesResult = properties;
		_disposed = false;
	}

	/// <summary>
	/// Internal ctor for pooling
	/// </summary>
	internal PropertyValidationResultCollection() { }

	internal void Reset(int propertiesCount, int messagesPerProperty)
	{
		_disposed = false;
		_messagesPerProperty = messagesPerProperty;
		_messages = ValidationMessagePool.Rent(propertiesCount * messagesPerProperty);
		_propertiesResult = PropertyValidationResultPool.Rent(propertiesCount);
		_count = 0;
	}

	internal PropertyValidationResult InitProperty(string name, string displayName)
	{
		var propertyResult = _propertiesResult[_count++] ??=
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			new PropertyValidationResult();

		int start = (_count - 1) * _messagesPerProperty;
		propertyResult.ResetProperty(name, displayName, _messages, start, start + _messagesPerProperty, 0);

		return propertyResult;
	}

	/// <summary>
	/// Concat this collection with another one. Items from the other collection are added to the end of this collection.
	/// </summary>
	/// <param name="collection"></param>
	/// <returns></returns>
	public PropertyValidationResultCollection Concat(PropertyValidationResultCollection collection)
	{
		if (_disposed)
		{
			throw new InvalidOperationException();
		}

		for (int index = 0; index < collection._count && _count < _messages.Length; index++)
		{
			_propertiesResult[_count++] = collection._propertiesResult[index];
		}

		// Reset counts of original collection
		collection._count = 0;

		return this;
	}

	/// <summary>
	/// Add property validation result to the collection
	/// </summary>
	/// <param name="propertyValidationResult"></param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Add(PropertyValidationResult propertyValidationResult)
	{
		if (_count >= _propertiesResult.Length)
		{
			throw new InvalidOperationException(
				"Collection is full. You probably created result for non-existing property."
			);
		}

		var existingPropertyResult = FindProperty(propertyValidationResult.PropertyPath);

		if (existingPropertyResult is null)
		{
			_propertiesResult[_count++] = propertyValidationResult;
			return;
		}

		var expandable = existingPropertyResult.AsExpandable();

		foreach (var message in propertyValidationResult.Messages)
		{
			expandable.Add(message);
		}
	}

	/// <summary>
	/// Find property by its path
	/// </summary>
	/// <param name="propertyPath"></param>
	/// <returns></returns>
	public PropertyValidationResult? FindProperty(string propertyPath)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_propertiesResult[i].PropertyPath == propertyPath)
			{
				return _propertiesResult[i];
			}
		}

		return null;
	}

	/// <inheritdoc />
	public IEnumerator<PropertyValidationResult> GetEnumerator()
	{
		for (int i = 0; i < _count; i++)
		{
			yield return _propertiesResult[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <inheritdoc />
	public PropertyValidationResult this[int index]
	{
		get
		{
			if (index < 0 || index > _count)
			{
				throw new IndexOutOfRangeException();
			}

			return _propertiesResult[index];
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_messages is not null)
		{
			ValidationMessagePool.Return(_messages, clearArray: false);
		}

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_propertiesResult is not null)
		{
			PropertyValidationResultPool.Return(_propertiesResult, clearArray: false);
		}

		_disposed = true;
	}
}
