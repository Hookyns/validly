using System.Text;
using Validly.Naming;
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
	/// CLR name of the validated property
	/// </summary>
	private string _propertyName;

	/// <summary>
	/// Explicit JSON name of the validated property (from [JsonPropertyName])
	/// </summary>
	private string? _propertyJsonName;

	/// <summary>
	/// Parent segments of the path (root first); null if the property is not nested
	/// </summary>
	private PropertyPathSegment? _parentSegments;

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

		// Split the path into segments; there are no explicit JSON names available
		int lastSeparator = propertyPath.LastIndexOf('/');
		_propertyName = propertyPath.Substring(lastSeparator + 1);

		if (lastSeparator > 0)
		{
			string[] parents = propertyPath.Substring(0, lastSeparator).Split('/');

			for (int index = parents.Length - 1; index >= 0; index--)
			{
				_parentSegments = new PropertyPathSegment(parents[index], null, _parentSegments);
			}
		}

		_messages = new SpanCollection<ValidationMessage>(messages.ToArray(), 0, messages.Count, messages.Count);
	}

	/// <summary>
	/// Create a new instance of <see cref="PropertyValidationResult"/> for pooling
	/// </summary>
	internal PropertyValidationResult()
	{
		PropertyPath = null!;
		PropertyDisplayName = null!;
		_propertyName = null!;
	}

	internal void ResetProperty(
		string propertyPath,
		string propertyDisplayName,
		string? propertyJsonName,
		ValidationMessage[] messages,
		int messagesStartPosition,
		int messagesEndPosition,
		int messagesCount
	)
	{
		PropertyPath = propertyPath;
		PropertyDisplayName = propertyDisplayName;
		_propertyName = propertyPath;
		_propertyJsonName = propertyJsonName;
		_parentSegments = null;
		_messages.Reset(messages, messagesStartPosition, messagesEndPosition, messagesCount);
	}

	/// <summary>
	/// Returns path of the validated property from the root object, as it appears in JSON.
	/// Segments are separated by "/" and escaped according to RFC 6901 (JSON pointer).
	/// </summary>
	/// <remarks>
	/// Each segment uses the explicit JSON name ([JsonPropertyName]) if set;
	/// otherwise the CLR name converted by the <paramref name="namingPolicy"/>.
	/// </remarks>
	/// <param name="namingPolicy">Naming policy applied to properties without an explicit JSON name</param>
	/// <returns></returns>
	public string GetJsonPointerPath(PropertyNamingPolicy? namingPolicy = null)
	{
		string leaf = EscapeJsonPointerSegment(ResolveJsonName(_propertyName, _propertyJsonName, namingPolicy));

		if (_parentSegments is null)
		{
			return leaf;
		}

		// One level of nesting is the common case; concat it directly instead of using a StringBuilder
		if (_parentSegments.Next is null)
		{
			return string.Concat(
				EscapeJsonPointerSegment(ResolveJsonName(_parentSegments.Name, _parentSegments.JsonName, namingPolicy)),
				"/",
				leaf
			);
		}

		var sb = new StringBuilder();

		for (PropertyPathSegment? segment = _parentSegments; segment is not null; segment = segment.Next)
		{
			sb.Append(EscapeJsonPointerSegment(ResolveJsonName(segment.Name, segment.JsonName, namingPolicy)));
			sb.Append('/');
		}

		return sb.Append(leaf).ToString();
	}

	private static string ResolveJsonName(string name, string? jsonName, PropertyNamingPolicy? namingPolicy)
	{
		return jsonName ?? namingPolicy?.ConvertName(name) ?? name;
	}

	private static string EscapeJsonPointerSegment(string segment)
	{
		if (segment.IndexOf('~') == -1 && segment.IndexOf('/') == -1)
		{
			return segment;
		}

		return segment.Replace("~", "~0").Replace("/", "~1");
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
		return ((IInternalPropertyValidationResult)this).AsNestedPropertyValidationResult(parentPropertyName, null);
	}

	/// <summary>
	/// Take the path of the source property result, optionally nested under the parent property
	/// </summary>
	/// <remarks>Path segments are immutable, so they can be shared with the source.</remarks>
	internal void CopyPathFrom(
		PropertyValidationResult source,
		string? parentPropertyName,
		string? parentPropertyJsonName,
		string? indexName = null
	)
	{
		_propertyName = source._propertyName;
		_propertyJsonName = source._propertyJsonName;
		PropertyPath = CreateNestedPath(source, parentPropertyName, indexName);

		PropertyPathSegment? segments = source._parentSegments;

		// Index of a collection item is used as it is in the JSON pointer; the naming policy does not apply
		if (indexName is not null)
		{
			segments = new PropertyPathSegment(indexName, indexName, segments);
		}

		if (parentPropertyName is not null)
		{
			segments = new PropertyPathSegment(parentPropertyName, parentPropertyJsonName, segments);
		}

		_parentSegments = segments;
	}

	/// <summary>
	/// Path of the source property result nested under the parent property (and the index of the collection item)
	/// </summary>
	internal static string CreateNestedPath(PropertyValidationResult source, string? parentPropertyName, string? indexName)
	{
		if (parentPropertyName is null)
		{
			return source.PropertyPath;
		}

		return indexName is null
			? $"{parentPropertyName}/{source.PropertyPath}"
			: $"{parentPropertyName}/{indexName}/{source.PropertyPath}";
	}

	/// <summary>
	/// Set the path of the property result
	/// </summary>
	/// <param name="propertyPath">Full path</param>
	/// <param name="propertyName">Name of the property (leaf segment)</param>
	/// <param name="propertyJsonName">Explicit JSON name of the leaf segment</param>
	/// <param name="parentSegments">Parent segments, linked from the root</param>
	internal void SetPath(
		string propertyPath,
		string propertyName,
		string? propertyJsonName,
		PropertyPathSegment? parentSegments
	)
	{
		PropertyPath = propertyPath;
		_propertyName = propertyName;
		_propertyJsonName = propertyJsonName;
		_parentSegments = parentSegments;
	}

	PropertyValidationResult IInternalPropertyValidationResult.AsNestedPropertyValidationResult(
		string parentPropertyName,
		string? parentPropertyJsonName
	)
	{
		PropertyPath = $"{parentPropertyName}/{PropertyPath}";
		_parentSegments = new PropertyPathSegment(parentPropertyName, parentPropertyJsonName, _parentSegments);
		return this;
	}
}
