using System.Collections;
using System.Runtime.CompilerServices;

namespace Validly.Utils;

/// <summary>
/// Read-only collection working as a Span
/// </summary>
/// <typeparam name="TItem"></typeparam>
public sealed class SpanCollection<TItem> : IReadOnlyList<TItem>, ICollection<TItem>
{
	private TItem[] _list;
	private int _startPosition;
	private int _endPosition;
	private int _count;

	/// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
	public int Count => _count;

	// Even though this is not exactly true, we will tell this is read-only. It's not ReadOnly just for internal use.
	/// <inheritdoc />
	public bool IsReadOnly => true;

	internal TItem[] BufferArray => _list;

	/// <param name="list"></param>
	/// <param name="startPosition">Beginning of the buffer</param>
	/// <param name="endPosition">End of the buffer</param>
	/// <param name="count">Number of items in the buffer</param>
	public SpanCollection(TItem[] list, int startPosition, int endPosition, int count)
	{
		_list = list;
		_startPosition = startPosition;
		_endPosition = endPosition;
		_count = count;
	}

	/// <summary>
	/// Reset/change backing array
	/// </summary>
	/// <param name="list"></param>
	/// <param name="startPosition"></param>
	/// <param name="endPosition"></param>
	/// <param name="count"></param>
	public void Reset(TItem[] list, int startPosition, int endPosition, int count)
	{
		_list = list;
		_startPosition = startPosition;
		_endPosition = endPosition;
		_count = count;
	}

	/// <inheritdoc />
	public IEnumerator<TItem> GetEnumerator()
	{
		for (int index = _startPosition; index < _startPosition + _count; index++)
		{
			yield return _list[index];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <inheritdoc />
#if NET7_0_OR_GREATER
	[System.Runtime.CompilerServices.MethodImpl(
		System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization
	)]
#endif
	public void Add(TItem item)
	{
		if (_count > (_endPosition - _startPosition))
		{
			throw new IndexOutOfRangeException("Collection is full.");
		}

		_list[_startPosition + _count] = item;
		_count++;
	}

	/// <inheritdoc />
	public void Clear()
	{
		_count = 0;
	}

	/// <inheritdoc />
#if NET7_0_OR_GREATER
	[System.Runtime.CompilerServices.MethodImpl(
		System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization
	)]
#endif
	public bool Contains(TItem item)
	{
		for (int index = _startPosition; index < _startPosition + _count; index++)
		{
			if (_list[index]?.Equals(item) == true)
			{
				return true;
			}
		}

		return false;
	}

	/// <inheritdoc />
	public void CopyTo(TItem[] array, int arrayIndex)
	{
		_list.AsSpan(_startPosition, _count).CopyTo(array.AsSpan(arrayIndex));
	}

	/// <inheritdoc />
	public bool Remove(TItem item)
	{
		// We cannot allow this. We would have to shift all the items.
		throw new InvalidOperationException("Not implemented.");
	}

	/// <inheritdoc />
	public TItem this[int index]
	{
		get
		{
			if (index < 0 || index >= _count)
			{
				throw new IndexOutOfRangeException();
			}

			return _list[_startPosition + index];
		}
	}
}
