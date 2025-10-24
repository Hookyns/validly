using System.Diagnostics.CodeAnalysis;

namespace Validly;

/// <summary>
/// Object representing the result of a validators' IsValid method
/// </summary>
public sealed class Validation
{
	private static readonly Validation SuccessValidation = new();

	/// <summary>
	/// True if the validation was successful
	/// </summary>
	[MemberNotNullWhen(false, "Message")]
	public bool IsSuccess => Message is null;

	/// <summary>
	/// The message associated with the validation if it failed
	/// </summary>
	public ValidationMessage? Message { get; private set; }

	private Validation() { }

	/// <summary>
	/// Creates a positive validation result
	/// </summary>
	/// <returns></returns>
	public static Validation Success() => SuccessValidation;

	/// <summary>
	/// Creates a negative validation result
	/// </summary>
	/// <returns></returns>
	public static Validation Error(ValidationMessage message) => new() { Message = message };

	/// <summary>
	/// Creates a negative validation result
	/// </summary>
	/// <returns></returns>
	public static Validation Error(
		[StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
		string resourceKey,
		params object?[] args
	) => new() { Message = new ValidationMessage(message, resourceKey, args) };
}
