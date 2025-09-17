namespace Validly;

/// <summary>
/// Interface for validatable objects
/// </summary>
public interface IValidatable
{
	/// <summary>
	/// Validate the object
	/// </summary>
	/// <param name="serviceProvider"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	ValueTask<ValidationResult> ValidateAsync(IServiceProvider serviceProvider, CancellationToken ct = default);
}
