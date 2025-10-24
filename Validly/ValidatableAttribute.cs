namespace Validly;

/// <summary>
/// Attribute to mark a class as validatable
/// </summary>
/// <remarks>
/// It serves as a marker for a source generator to know that the class is validatable.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ValidatableAttribute : Attribute
{
	/// <summary>
	/// Enables auto validators for the class
	/// </summary>
	public bool UseAutoValidators { get; set; }

	/// <summary>
	/// Disables auto validators for the class
	/// </summary>
	public bool NoAutoValidators { get; set; }

	/// <summary>
	/// Enables exit early for the class
	/// </summary>
	public bool UseExitEarly { get; set; }

	/// <summary>
	/// Disables exit early for the class
	/// </summary>
	public bool NoExitEarly { get; set; }
}
