using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Validly.Utils;

namespace Validly;

/// <summary>
/// Represents the result of a validation. Variant with methods for adding global messages and properties results.
/// </summary>
public class ExtendableValidationResult : ValidationResult, IResettable
{
	private static readonly FinalizableObjectPool<ExtendableValidationResult> Pool =
		FinalizableObjectPool.Create<ExtendableValidationResult>();
	private static readonly ArrayPool<ValidationMessage> GlobalMessagePool = ArrayPool<ValidationMessage>.Shared;
	private static readonly ArrayPool<PropertyValidationResult> PropertyValidationResultPool =
		ArrayPool<PropertyValidationResult>.Shared;

	private bool _disposed;

	// /// <summary>
	// /// Represents the result of a validation. Variant with methods for adding global messages and properties results.
	// /// </summary>
	// public ExtendableValidationResult(int propertiesCount)
	// 	: base(new List<ValidationMessage>(), new List<PropertyValidationResult>(propertiesCount))
	// {
	// }

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
		GlobalMessages = GlobalMessagePool.Rent(ValidlyOptions.GlobalMessagesPoolSize);
		PropertiesResult = PropertyValidationResultPool.Rent(propertiesCount);
	}

	/// <inheritdoc />
	bool IResettable.TryReset()
	{
		PropertiesResult = null!;
		GlobalMessages = null!;
		GlobalMessagesCount = 0;
		PropertiesResultCount = 0;
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
			GlobalMessagePool.Return(GlobalMessages);
			GlobalMessages = null!;
		}

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (PropertiesResult is not null)
		{
			for (int index = 0; index < PropertiesResultCount; index++)
			{
				PropertyValidationResult propertyValidationResult = PropertiesResult[index];
				propertyValidationResult.Dispose();
			}

			PropertyValidationResultPool.Return(PropertiesResult);
			PropertiesResult = null!;
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

	/// <summary>
	/// Add a global message to the validation result
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public ExtendableValidationResult AddGlobalMessage(ValidationMessage message)
	{
		AddGlobalMessageToArray(message);
		return this;
	}

	/// <summary>
	/// Add a global messages to the validation result
	/// </summary>
	/// <param name="messages"></param>
	/// <returns></returns>
	public ExtendableValidationResult AddGlobalMessages(IEnumerable<ValidationMessage> messages)
	{
		foreach (ValidationMessage? message in messages)
		{
			AddGlobalMessageToArray(message);
		}

		return this;
	}

	/// <summary>
	/// Add a global messages to the validation result
	/// </summary>
	/// <param name="messages"></param>
	/// <returns></returns>
	public async ValueTask<ExtendableValidationResult> AddGlobalMessages(IAsyncEnumerable<ValidationMessage> messages)
	{
		await foreach (ValidationMessage message in messages)
		{
			AddGlobalMessageToArray(message);
		}

		return this;
	}

	/// <summary>
	/// Add a property result to the validation result
	/// </summary>
	public ExtendableValidationResult AddPropertyResult(PropertyValidationResult propertyResult)
	{
		AddPropertyResultToArray(propertyResult);
		return this;
	}

	/// <summary>
	/// Combine results. Global messages and property results are added to the current result.
	/// </summary>
	/// <param name="result"></param>
	public void Combine(ValidationResult result)
	{
		// POP all messages from the result and add them to the current result
		for (int index = 0; index < result.GlobalMessagesCount; index++)
		{
			ValidationMessage message = result.GlobalMessages[index];
			AddGlobalMessageToArray(message);
		}

		// Reset count
		result.GlobalMessagesCount = 0;

		// POP all properties from the result and add them to the current result
		for (int index = 0; index < result.PropertiesResultCount; index++)
		{
			PropertyValidationResult propertyResult = result.PropertiesResult[index];
			result.PropertiesResult[index] = null!;
			AddPropertyResultToArray(propertyResult);
		}

		// Reset count
		result.PropertiesResultCount = 0;
	}

	/// <summary>
	/// Combine results. Global messages and property results are added to the current result.
	/// </summary>
	/// <param name="result"></param>
	/// <param name="propertyName">Name of the property from which the result comes from.</param>
	public void CombineNested(ValidationResult result, string propertyName)
	{
		// POP all messages from the result and add them to the current result
		for (int index = 0; index < result.GlobalMessagesCount; index++)
		{
			ValidationMessage message = result.GlobalMessages[index];
			AddGlobalMessageToArray(message);
		}

		// Reset count
		result.GlobalMessagesCount = 0;

		// POP all properties from the result and add them to the current result
		for (int index = 0; index < result.PropertiesResultCount; index++)
		{
			PropertyValidationResult propertyResult = result.PropertiesResult[index];
			((IInternalPropertyValidationResult)propertyResult).AsNestedPropertyValidationResult(propertyName);
			result.PropertiesResult[index] = null!;
			AddPropertyResultToArray(propertyResult);
		}

		// Reset count
		result.PropertiesResultCount = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddGlobalMessageToArray(ValidationMessage message)
	{
		if (GlobalMessagesCount < GlobalMessages.Length)
		{
			GlobalMessages[GlobalMessagesCount++] = message;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddPropertyResultToArray(PropertyValidationResult propertyResult)
	{
		if (PropertiesResultCount < PropertiesResult.Length)
		{
			PropertiesResult[PropertiesResultCount++] = propertyResult;
		}
	}
}
