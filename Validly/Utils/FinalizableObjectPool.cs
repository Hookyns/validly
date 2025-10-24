using System.Collections.Concurrent;
using Microsoft.Extensions.ObjectPool;

namespace Validly.Utils;

internal sealed class FinalizableObjectPool<TItem>
	where TItem : class
{
	private readonly IPooledObjectPolicy<TItem> _policy;
	private readonly int _maxCapacity;

	private TItem? _fastItem;

	private readonly ConcurrentQueue<TItem> _items = new();
	private volatile int _numItems;

	public FinalizableObjectPool(IPooledObjectPolicy<TItem> policy, int maximumRetained)
	{
		_policy = policy;
		_maxCapacity = maximumRetained - 1; // -1 for _fastItem
	}

	public TItem Get()
	{
		var item = _fastItem;

		if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
		{
			if (_items.TryDequeue(out item))
			{
				Interlocked.Decrement(ref _numItems);
				return item;
			}

			// Create a new object if the pool is empty
			return _policy.Create();
		}

		return item;
	}

	/// <summary>
	/// Returns an object back to the pool
	/// </summary>
	/// <returns>Returns true if the object was returned to the pool</returns>
	public bool Return(TItem obj)
	{
		if (!_policy.Return(obj))
		{
			// Rejected by policy
			return false;
		}

		if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, obj, null) != null)
		{
			if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
			{
				_items.Enqueue(obj);
				return true;
			}

			// Max number of items reached, clean up the count and reject the object
			Interlocked.Decrement(ref _numItems);
			return false;
		}

		return true;
	}
}

internal static class FinalizableObjectPool
{
	public static FinalizableObjectPool<TItem> Create<TItem>(IPooledObjectPolicy<TItem>? policy = null)
		where TItem : class, IResettable, new()
	{
		return new FinalizableObjectPool<TItem>(
			policy ?? new ResettablePooledObjectPolicy<TItem>(),
			ValidlyOptions.ObjectPoolSize
		);
	}
}

/// <summary>
/// Resettable pooled object policy
/// </summary>
/// <typeparam name="TItem"></typeparam>
internal class ResettablePooledObjectPolicy<TItem> : PooledObjectPolicy<TItem>
	where TItem : class, IResettable, new()
{
	/// <inheritdoc />
	public override TItem Create()
	{
		return new TItem();
	}

	/// <inheritdoc />
	public override bool Return(TItem obj)
	{
		return obj.TryReset();
	}
}
