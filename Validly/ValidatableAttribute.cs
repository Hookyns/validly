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

	/// <summary>
	/// The class does not implement Validly interfaces (<see cref="IValidatable"/>); projects that use the class but
	/// do not validate it do not have to reference Validly. The Validate/ValidateAsync methods are generated as usual.
	/// The object is validatable by its runtime type through <see cref="ValidatorRegistry"/> (requires
	/// ModuleInitializerAttribute, .NET 5+); nested detached objects are validated without polymorphism.
	/// </summary>
	public bool Detached { get; set; }
}
