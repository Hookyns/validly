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
	/// True when the arrays are rented from the pools (and must be returned); false for user-provided arrays
	/// </summary>
	private bool _pooled;

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
		_count = properties.Length;
		_disposed = false;
	}

	/// <summary>
	/// Internal ctor for pooling
	/// </summary>
	internal PropertyValidationResultCollection() { }

	internal void Reset(int propertiesCount, int messagesPerProperty)
	{
		_disposed = false;
		_pooled = true;
		_messagesPerProperty = messagesPerProperty;
		_messages = ValidationMessagePool.Rent(propertiesCount * messagesPerProperty);
		_propertiesResult = PropertyValidationResultPool.Rent(propertiesCount);
		_count = 0;
	}

	internal PropertyValidationResult InitProperty(string name, string displayName, string? jsonName)
	{
		if (_count >= _propertiesResult.Length)
		{
			throw new InvalidOperationException(
				"Collection is full. You probably created result for non-existing property."
			);
		}

		// PropertyValidationResult objects are owned by the pooled array they live in; they are never shared
		// with another collection (results are copied between collections), so they can be safely reused.
		var propertyResult = _propertiesResult[_count++] ??=
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			new PropertyValidationResult();

		int start = (_count - 1) * _messagesPerProperty;
		propertyResult.ResetProperty(name, displayName, jsonName, _messages, start, start + _messagesPerProperty, 0);

		return propertyResult;
	}

	/// <summary>
	/// Concat this collection with another one. Items from the other collection are copied to the end of this collection.
	/// </summary>
	/// <param name="collection"></param>
	/// <returns></returns>
	public PropertyValidationResultCollection Concat(PropertyValidationResultCollection collection)
	{
		if (_disposed)
		{
			throw new InvalidOperationException();
		}

		for (int index = 0; index < collection._count; index++)
		{
			PropertyValidationResult source = collection._propertiesResult[index];
			AddCopy(source, null, null, findExisting: false);
		}

		return this;
	}

	/// <summary>
	/// Add property validation result to the collection
	/// </summary>
	/// <remarks>
	/// Messages of a property with the same path are merged into the existing property result.
	/// For pooled collections, the result is copied, so the passed instance is not referenced by the collection.
	/// </remarks>
	/// <param name="propertyValidationResult"></param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Add(PropertyValidationResult propertyValidationResult)
	{
		if (_pooled)
		{
			AddCopy(propertyValidationResult, null, null, findExisting: true);
			return;
		}

		var existingPropertyResult = FindProperty(propertyValidationResult.PropertyPath);

		if (existingPropertyResult is null)
		{
			if (_count >= _propertiesResult.Length)
			{
				throw new InvalidOperationException(
					"Collection is full. You probably created result for non-existing property."
				);
			}

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
	/// Copy the property result (its messages) into a property result owned by this collection
	/// </summary>
	/// <param name="source">Property result to copy; it is not referenced by this collection afterward</param>
	/// <param name="parentPropertyName">Parent property the source is nested under; null to keep the path</param>
	/// <param name="parentPropertyJsonName">Explicit JSON name of the parent property</param>
	/// <param name="findExisting">Merge messages into an existing property result with the same path</param>
	internal void AddCopy(
		PropertyValidationResult source,
		string? parentPropertyName,
		string? parentPropertyJsonName,
		bool findExisting
	)
	{
		PropertyValidationResult? target = null;

		if (findExisting)
		{
			target = FindProperty(
				parentPropertyName is null ? source.PropertyPath : $"{parentPropertyName}/{source.PropertyPath}"
			);
		}

		if (target is null)
		{
			target = InitProperty(source.PropertyPath, source.PropertyDisplayName, null);
			target.CopyPathFrom(source, parentPropertyName, parentPropertyJsonName);
		}

		IReadOnlyList<ValidationMessage> messages = source.Messages;
		var expandable = (IExpandablePropertyValidationResult)target;

		for (int index = 0; index < messages.Count; index++)
		{
			expandable.Add(messages[index]);
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
			if (index < 0 || index >= _count)
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

		_disposed = true;
		_count = 0;

		// User-provided arrays must not get into the shared pools
		if (!_pooled)
		{
			return;
		}

		_pooled = false;

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_messages is not null)
		{
			ValidationMessagePool.Return(_messages, clearArray: false);
		}

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_propertiesResult is not null)
		{
			// Not cleared on purpose; PropertyValidationResult objects are reused by the next renter.
			// They are owned by this array only, results are always copied between collections.
			PropertyValidationResultPool.Return(_propertiesResult, clearArray: false);
		}

		// ReSharper disable UseCollectionExpression
		_messages = Array.Empty<ValidationMessage>();
		_propertiesResult = Array.Empty<PropertyValidationResult>();
		// ReSharper restore UseCollectionExpression
	}
}
