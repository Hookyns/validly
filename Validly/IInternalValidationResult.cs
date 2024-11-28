namespace Validly;

/// <summary>
/// Internal interface for validation results
/// </summary>
public interface IInternalValidationResult
{
	/// <summary>
	/// Returns number of properties in this validation result
	/// </summary>
	/// <returns></returns>
	int GetPropertiesCount();

	/// <summary>
	/// Take property from the pool and initialize it
	/// </summary>
	/// <param name="name"></param>
	/// <param name="displayName"></param>
	/// <returns></returns>
	IExpandablePropertyValidationResult InitProperty(string name, string? displayName = null);

	/// <summary>
	/// Add a global message to the validation result
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	void Add(Validation? message);

	/// <summary>
	/// Add a global message to the validation result
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	void Add(ValidationMessage? message);

	/// <summary>
	/// Add a global messages to the validation result
	/// </summary>
	/// <param name="messages"></param>
	/// <returns></returns>
	void Add(IEnumerable<ValidationMessage> messages);

	/// <summary>
	/// Add a global messages to the validation result
	/// </summary>
	/// <param name="messages"></param>
	/// <returns></returns>
	Task AddAsync(IAsyncEnumerable<ValidationMessage> messages);

	/// <summary>
	/// Combine results. Global messages and property results are added to the current result.
	/// </summary>
	/// <param name="result"></param>
	void Combine(ValidationResult result);

	/// <summary>
	/// Combine results. Global messages and property results are added to the current result.
	/// </summary>
	/// <param name="result"></param>
	/// <param name="propertyName">Name of the property from which the result comes from.</param>
	void CombineNested(ValidationResult result, string propertyName);
}
