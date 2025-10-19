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
}

public sealed record DependencyInjectionInfo(string Name, bool IsKeyedService, object? Key);