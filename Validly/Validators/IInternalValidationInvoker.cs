namespace Validly.Validators;

/// <summary>
/// Interface for internal use to communicate with validators of nested objects
/// </summary>
public interface IInternalValidationInvoker
{
	/// <summary>
	/// Gets a validation context from validated object
	/// </summary>
	/// <returns></returns>
	ValidationContext? GetValidationContext();

	/// <summary>
	/// Sets the validation context to be used
	/// </summary>
	/// <param name="validationContext"></param>
	void SetValidationContext(ValidationContext validationContext);
}
