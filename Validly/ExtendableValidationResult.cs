using System.Buffers;
using Microsoft.Extensions.ObjectPool;
using Validly.Utils;

namespace Validly;

/// <summary>
/// Represents the result of a validation. Variant with methods for adding global messages and properties results.
/// </summary>
public sealed class ExtendableValidationResult : ValidationResult, IResettable
{
	private static readonly FinalizableObjectPool<ExtendableValidationResult> Pool =
		FinalizableObjectPool.Create<ExtendableValidationResult>();
	private static readonly ArrayPool<ValidationMessage> GlobalMessagePool = ArrayPool<ValidationMessage>.Shared;

	private bool _disposed;

	static ExtendableValidationResult() { }

	/// <summary>
	/// Ctor for pooled objects
	/// </summary>
	[Obsolete("Use Create method instead.")]
	public ExtendableValidationResult() { }

	/// <summary>
	/// Creates new instance of pooled <see cref="ExtendableValidationResult"/>
	/// </summary>
	/// <param name="propertiesCount"></param>
	/// <returns></returns>
	public static ExtendableValidationResult Create(int propertiesCount)
	{
		var result = Pool.Get();
		result.Reset(propertiesCount);

		return result;
	}

	private void Reset(int propertiesCount)
	{
		_disposed = false;
		var messages = GlobalMessagePool.Rent(ValidlyOptions.GlobalMessagesPoolSize);
		GlobalMessages.Reset(messages, 0, messages.Length, 0);
		PropertiesResultCollection.Reset(propertiesCount, ValidlyOptions.PropertyMessagesPoolSize);
	}

	/// <inheritdoc />
	bool IResettable.TryReset()
	{
		GlobalMessages.Clear();
		return true;
	}

	/// <summary>
	/// Tries to return the object to the pool; otherwise it will dispose it so it can be garbage collected.
	/// </summary>
	/// <param name="disposing"></param>
	/// <returns>Returns true when object is disposed (and not returned to the pool).</returns>
	protected override bool Dispose(bool disposing)
	{
		if (_disposed)
		{
			return true;
		}

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (GlobalMessages is not null)
		{
			GlobalMessagePool.Return(GlobalMessages.BufferArray);
		}

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (PropertiesResultCollection is not null)
		{
			PropertiesResultCollection.Dispose();
		}

		_disposed = true;

		return !Pool.Return(this);
	}

	/// <summary>
	/// Dispose the object using finalize for case developer forgets to dispose it.
	/// </summary>
	~ExtendableValidationResult()
	{
		if (!Dispose(false))
		{
			// Reregister for finalization if not disposed (returned to the pool)
			GC.ReRegisterForFinalize(this);
		}
	}
}
