using Validly.Utils;

namespace Validly;

/// <summary>
/// Object holding the result of a property validation
/// </summary>
public sealed class PropertyValidationResult : IInternalPropertyValidationResult, IExpandablePropertyValidationResult
{
	/// <summary>
	/// Error messages generated during validation
	/// </summary>
	// ReSharper disable once UseCollectionExpression
	private readonly SpanCollection<ValidationMessage> _messages = new(Array.Empty<ValidationMessage>(), 0, 0, 0);

	/// <summary>
	/// Path of the validated property from the root object
	/// </summary>
	public string PropertyPath { get; private set; }

	/// <summary>
	/// DisplayName of the validated property
	/// </summary>
	public string PropertyDisplayName { get; private set; }

	/// <summary>
	/// Error messages generated during validation
	/// </summary>
	public IReadOnlyList<ValidationMessage> Messages => _messages;

	/// <summary>
	/// True if validation of this property was successful
	/// </summary>
	public bool IsSuccess => _messages.Count == 0;

	/// <param name="propertyPath"></param>
	/// <param name="propertyDisplayName"></param>
	/// <param name="messages"></param>
	public PropertyValidationResult(
		string propertyPath,
		string propertyDisplayName,
		IReadOnlyList<ValidationMessage> messages
	)
	{
		PropertyPath = propertyPath;
		PropertyDisplayName = propertyDisplayName;

		_messages = new SpanCollection<ValidationMessage>(messages.ToArray(), 0, messages.Count - 1, messages.Count);
	}

	/// <summary>
	/// Create a new instance of <see cref="PropertyValidationResult"/> for pooling
	/// </summary>
	internal PropertyValidationResult()
	{
		PropertyPath = null!;
		PropertyDisplayName = null!;
	}

	internal void ResetProperty(
		string propertyPath,
		string propertyDisplayName,
		ValidationMessage[] messages,
		int messagesStartPosition,
		int messagesEndPosition,
		int messagesCount
	)
	{
		PropertyPath = propertyPath;
		PropertyDisplayName = propertyDisplayName;
		_messages.Reset(messages, messagesStartPosition, messagesEndPosition, messagesCount);
	}

	/// <summary>
	/// Cast the validation result to an expandable validation result so you can add more messages to it
	/// </summary>
	/// <returns></returns>
	public IExpandablePropertyValidationResult AsExpandable()
	{
		return this;
	}

	void IExpandablePropertyValidationResult.Add(ValidationMessage? message)
	{
		if (message is null)
		{
			return;
		}

		_messages.Add(message);
	}

	void IExpandablePropertyValidationResult.Add(Validation? message)
	{
		if (message is null)
		{
			return;
		}

		if (message.IsSuccess == false)
		{
			_messages.Add(message.Message);
		}
	}

	void IExpandablePropertyValidationResult.Add(IEnumerable<ValidationMessage> messages)
	{
		foreach (ValidationMessage message in messages)
		{
			_messages.Add(message);
		}
	}

	async Task IExpandablePropertyValidationResult.AddAsync(IAsyncEnumerable<ValidationMessage> messages)
	{
		await foreach (ValidationMessage message in messages)
		{
			_messages.Add(message);
		}
	}

	// /// <summary>
	// /// Add message to the validation result
	// /// </summary>
	// /// <param name="message"></param>
	// /// <returns></returns>
	// public void AddValidationMessage(ValidationMessage? message)
	// {
	// 	if (message is not null && _messagesArrayItemCount < _messagesArray.Length)
	// 	{
	// 		_messagesArray[_messagesArrayItemCount++] = message;
	// 	}
	// }
	//
	// /// <summary>
	// /// Add message to the validation result
	// /// </summary>
	// /// <param name="message"></param>
	// /// <returns></returns>
	// public void AddValidationMessage(Validation? message)
	// {
	// 	if (message?.IsSuccess == false && _messagesArrayItemCount < _messagesArray.Length)
	// 	{
	// 		_messagesArray[_messagesArrayItemCount++] = message.Message;
	// 	}
	// }
	//
	// /// <summary>
	// /// Add messages to the validation result
	// /// </summary>
	// /// <param name="messages"></param>
	// /// <returns></returns>
	// public void AddValidationMessages(IEnumerable<ValidationMessage> messages)
	// {
	// 	foreach (ValidationMessage message in messages)
	// 	{
	// 		if (_messagesArrayItemCount >= _messagesArray.Length)
	// 		{
	// 			break;
	// 		}
	//
	// 		_messagesArray[_messagesArrayItemCount++] = message;
	// 	}
	// }
	//
	// /// <summary>
	// /// Add messages to the validation result
	// /// </summary>
	// /// <param name="messages"></param>
	// /// <returns></returns>
	// public async Task AddValidationMessages(IAsyncEnumerable<ValidationMessage> messages)
	// {
	// 	await foreach (ValidationMessage message in messages)
	// 	{
	// 		if (_messagesArrayItemCount >= _messagesArray.Length)
	// 		{
	// 			break;
	// 		}
	//
	// 		_messagesArray[_messagesArrayItemCount++] = message;
	// 	}
	// }

	PropertyValidationResult IInternalPropertyValidationResult.AsNestedPropertyValidationResult(
		string parentPropertyName
	)
	{
		PropertyPath = $"{parentPropertyName}/{PropertyPath}";
		return this;
	}
}
