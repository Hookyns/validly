namespace Validly;

/// <summary>
/// Interface allowing some stuff intended for internal use
/// </summary>
public interface IInternalPropertyValidationResult
{
	/// <summary>
	/// This method will change the name of the property in the result.
	/// It will change PropertyPath to "ParentPropertyName/PropertyPath".
	/// </summary>
	/// <param name="parentPropertyName"></param>
	/// <returns></returns>
	PropertyValidationResult AsNestedPropertyValidationResult(string parentPropertyName);

	/// <summary>
	/// This method will change the name of the property in the result.
	/// It will change PropertyPath to "ParentPropertyName/PropertyPath".
	/// </summary>
	/// <param name="parentPropertyName"></param>
	/// <param name="parentPropertyJsonName">Explicit JSON name of the parent property (from [JsonPropertyName])</param>
	/// <returns></returns>
	PropertyValidationResult AsNestedPropertyValidationResult(string parentPropertyName, string? parentPropertyJsonName);
}
