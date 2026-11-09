using System.ComponentModel;

namespace Validly;

/// <summary>
/// Validation of a validatable object as a part of another validation (nested object, collection item).
/// Implemented explicitly by the generated code; not intended to be used directly.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IInternalValidatable
{
	/// <summary>
	/// Validate the object within the validation context of its parent
	/// </summary>
	/// <param name="context">Validation context of the root validation</param>
	/// <param name="serviceProvider">Service provider of the root validation; may be null</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Disposable validation result of the object</returns>
	ValueTask<ValidationResult> ValidlyValidateCoreAsync(
		ValidationContext context,
		IServiceProvider? serviceProvider,
		CancellationToken ct
	);
}
