using System.Runtime.CompilerServices;
using System.Text;
using Validly.Details;
using Validly.Utils;

namespace Validly;

/// <summary>
/// Represents the result of a validation
/// </summary>
public class ValidationResult : IDisposable, IInternalValidationResult
{
	private static readonly ValidationResult SuccessResult = new();

	/// <summary>
	/// Global validation messages; messages that are not tied to a specific property
	/// </summary>
	protected readonly SpanCollection<ValidationMessage> GlobalMessages = new(
		// ReSharper disable once UseCollectionExpression
		Array.Empty<ValidationMessage>(),
		0,
		0,
		0
	);

	/// <summary>
	/// Properties validation results
	/// </summary>
	protected readonly PropertyValidationResultCollection PropertiesResultCollection = new();

	/// <summary>
	/// When true, the object has been disposed and is inside the pool
	/// </summary>
	private bool _disposed;

	/// <summary>
	/// List of global validation messages
	/// </summary>
	public IReadOnlyList<ValidationMessage> Global => GlobalMessages;

	/// <summary>
	/// List of properties validation results
	/// </summary>
	public IReadOnlyList<PropertyValidationResult> Properties => PropertiesResultCollection;

	/// <summary>
	/// True if validation was successful
	/// </summary>
	public bool IsSuccess => GlobalMessages.Count == 0 && PropertiesResultCollection.IsSuccess;

	/// <summary>
	/// Represents the result of a validation
	/// </summary>
	/// <param name="globalMessages"></param>
	public ValidationResult(ValidationMessage[] globalMessages)
	{
		GlobalMessages = new(globalMessages, 0, globalMessages.Length - 1, globalMessages.Length);
	}

	/// <summary>
	/// Represents the result of a validation
	/// </summary>
	/// <param name="globalMessages"></param>
	public ValidationResult(ICollection<ValidationMessage> globalMessages)
		: this(globalMessages.ToArray()) { }

	/// <summary>
	/// Constructor for pooling
	/// </summary>
	/// <remarks>
	/// This ctor is danger. Not all fields are initialized.
	/// </remarks>
#pragma warning disable CS8618, CS9264
	protected ValidationResult() { }
#pragma warning restore CS8618, CS9264

	/// <summary>
	/// Success result
	/// </summary>
	public static ValidationResult Success() => SuccessResult;

	/// <summary>
	/// Copies data to a structure similar to the validation problem details defined in RFC 9457
	/// </summary>
	/// <returns></returns>
	public ValidationResultDetails GetProblemDetails()
	{
		return new ValidationResultDetails
		{
			Errors = PropertiesResultCollection
				.SelectMany(prop =>
					prop.Messages.Select(error => new ValidationErrorDetail
					{
						Detail = error.Message,
						ResourceKey = error.ResourceKey,
						Args = error.Args,
						Pointer = $"#{prop.PropertyPath}",
						FieldName = prop.PropertyDisplayName,
					})
				)
				.Concat(
					GlobalMessages.Select(error => new ValidationErrorDetail
					{
						Detail = error.Message,
						ResourceKey = error.ResourceKey,
						Args = error.Args,
						Pointer = "#",
						FieldName = null,
					})
				)
				.ToArray(),
		};
	}

	/// <summary>
	/// Generates JSON matching the validation problem details defined in RFC 9457
	/// </summary>
	/// <returns></returns>
	public string GetProblemDetailsJson()
	{
		// Create StringBuilder with estimated size
		var sb = new StringBuilder(180 + 200 * PropertiesResultCollection.Count + 200 * GlobalMessages.Count);

		for (int globalMessageIndex = 0; globalMessageIndex < GlobalMessages.Count; globalMessageIndex++)
		{
			ValidationMessage error = GlobalMessages[globalMessageIndex];

			sb.AppendLine(
				$$"""
				    {
				      "detail": "{{error.Message}}",
				      "resourceKey": "{{error.ResourceKey}}",
				      "args": [{{error.ArgsJson}}],
				      "pointer": "#"
				    },
				"""
			);
		}

		for (int index = 0; index < PropertiesResultCollection.Count; index++)
		{
			PropertyValidationResult prop = PropertiesResultCollection.ArrayBuffer[index];

			for (int messageIndex = 0; messageIndex < prop.Messages.Count; messageIndex++)
			{
				ValidationMessage error = prop.Messages[messageIndex];

				sb.AppendLine(
					$$"""
					    {
					      "detail": "{{error.Message}}",
					      "resourceKey": "{{error.ResourceKey}}",
					      "args": [{{error.ArgsJson}}],
					      "pointer": "#/{{prop.PropertyPath}}",
					      "fieldName": "{{prop.PropertyDisplayName}}"
					    },
					"""
				);
			}
		}

		return $$"""
			{
			  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
			  "title": "One or more validation errors occurred.",
			  "status": 400,
			  "errors": [
			{{sb.ToString().TrimEnd(',', ' ', '\n', '\r', '\t')}}
			  ]
			}
			""";
	}

	/// <inheritdoc />
	int IInternalValidationResult.GetPropertiesCount() => PropertiesResultCollection.Count;

	/// <inheritdoc />
	IExpandablePropertyValidationResult IInternalValidationResult.InitProperty(string name, string? displayName)
	{
		return PropertiesResultCollection.InitProperty(name, displayName ?? name);
	}

	/// <summary>
	/// Add a global message to the validation result
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void IInternalValidationResult.Add(Validation? message)
	{
		if (message is null || message.IsSuccess)
		{
			return;
		}

		AddGlobalMessageToArray(message.Message);
	}

	/// <summary>
	/// Add a global message to the validation result
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	void IInternalValidationResult.Add(ValidationMessage? message)
	{
		if (message is null)
		{
			return;
		}

		AddGlobalMessageToArray(message);
	}

	/// <summary>
	/// Add a global messages to the validation result
	/// </summary>
	/// <param name="messages"></param>
	/// <returns></returns>
	void IInternalValidationResult.Add(IEnumerable<ValidationMessage> messages)
	{
		foreach (ValidationMessage? message in messages)
		{
			AddGlobalMessageToArray(message);
		}
	}

	/// <summary>
	/// Add a global messages to the validation result
	/// </summary>
	/// <param name="messages"></param>
	/// <returns></returns>
	async Task IInternalValidationResult.AddAsync(IAsyncEnumerable<ValidationMessage> messages)
	{
		await foreach (ValidationMessage message in messages)
		{
			AddGlobalMessageToArray(message);
		}
	}

	void IInternalValidationResult.Combine(ValidationResult result)
	{
		// TODO: How about some CopyTo method? And what about result.GlobalMessages reset? (Cuz we reset result.PropertiesResultCollection but not the GlobalMessages)
		// POP all messages from the result and add them to the current result
		for (int index = 0; index < result.GlobalMessages.Count; index++)
		{
			ValidationMessage message = result.GlobalMessages[index];
			AddGlobalMessageToArray(message);
		}

		// Concat properties
		PropertiesResultCollection.Concat(result.PropertiesResultCollection);
	}

	void IInternalValidationResult.CombineNested(ValidationResult result, string propertyName)
	{
		// POP all messages from the result and add them to the current result
		for (int index = 0; index < result.GlobalMessages.Count; index++)
		{
			ValidationMessage message = result.GlobalMessages[index];
			AddGlobalMessageToArray(message);
		}

		// POP all properties from the result and add them to the current result
		for (int index = 0; index < result.PropertiesResultCollection.Count; index++)
		{
			PropertyValidationResult propertyResult = result.PropertiesResultCollection[index];
			((IInternalPropertyValidationResult)propertyResult).AsNestedPropertyValidationResult(propertyName);
			// result.PropertiesResultCollection[index] = null!;
			PropertiesResultCollection.Add(propertyResult);
		}

		// // Reset count
		// result.PropertiesResultCollection.Count = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddGlobalMessageToArray(ValidationMessage message)
	{
		GlobalMessages.Add(message);
	}

	/// <summary>
	/// Dispose
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual bool Dispose(bool disposing)
	{
		if (_disposed)
		{
			return false;
		}

		if (disposing)
		{
			PropertiesResultCollection.Dispose();
			GlobalMessages.Clear();
		}

		_disposed = true;

		return false;
	}

	/// <inheritdoc />
	[DangerAfterDispose]
	public void Dispose()
	{
		if (Dispose(true))
		{
			// If disposed, suppress finalization
			GC.SuppressFinalize(this);
		}
	}
}
