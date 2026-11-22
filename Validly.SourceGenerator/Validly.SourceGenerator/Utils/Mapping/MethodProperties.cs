namespace Validly.SourceGenerator.Utils.Mapping;

internal record MethodProperties
{
	/// <summary>
	/// Name of the method
	/// </summary>
	public required string MethodName { get; init; }

	/// <summary>
	/// Name of the return type
	/// </summary>
	public required string ReturnType { get; init; }

	/// <summary>
	/// Name of the return type
	/// </summary>
	public required ReturnTypeType ReturnTypeType { get; init; }

	/// <summary>
	/// Name of the return type's generic argument
	/// </summary>
	public required string? ReturnTypeGenericArgument { get; init; }

	/// <summary>
	/// Dependencies of the method
	/// </summary>
	public required EquatableArray<DependencyInjectionInfo> Dependencies { get; init; }

	/// <summary>
	/// The method overrides a method declared by a validatable ancestor; such a hook is called (virtually)
	/// by the validation of the ancestor, so it must not be called again
	/// </summary>
	public bool IsCalledByBase { get; init; }
}