namespace Validly.Validators;

/// <summary>
/// Interface for internal use to invoke validation of nested objects
/// </summary>
public interface IInternalValidationInvoker
{
	/// <summary>
	/// Validates the object
	/// </summary>
	/// <param name="validationContext"></param>
	/// <param name="serviceProvider"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	ValueTask<ValidationResult> ValidateAsync(ValidationContext validationContext, IServiceProvider? serviceProvider, CancellationToken ct);
}
