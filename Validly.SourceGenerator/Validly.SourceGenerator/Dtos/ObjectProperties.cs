using Microsoft.CodeAnalysis;
using Validly.SourceGenerator.Utils;
using Validly.SourceGenerator.Utils.Mapping;

namespace Validly.SourceGenerator.Dtos;

internal record ObjectProperties
{
	/// <summary>
	/// Enables/disables auto validators
	/// </summary>
	public required bool? UseAutoValidators { get; init; }

	/// <summary>
	/// Enables/disables exit-early behavior
	/// </summary>
	public required bool? ExitEarly { get; init; }

	/// <summary>
	/// Usings from the validator's file
	/// </summary>
	public required EquatableArray<string> Usings { get; init; }

	public required string ClassOrRecordKeyword { get; init; }

	/// <summary>
	/// Access modifier
	/// </summary>
	public required Accessibility Accessibility { get; init; }

	/// <summary>
	/// Namespace of the validator
	/// </summary>
	public required string? Namespace { get; init; }

	/// <summary>
	/// Name of the validator without the "Validator" suffix
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Object inherits
	/// </summary>
	public required bool InheritsValidatableObject { get; init; }

	/// <summary>
	/// Is sealed class
	/// </summary>
	public required bool IsSealed { get; init; }

	/// <summary>
	/// List of properties
	/// </summary>
	public required EquatableArray<PropertyProperties> Properties { get; init; }

	/// <summary>
	/// List of methods
	/// </summary>
	public required EquatableArray<MethodProperties> Methods { get; init; }

	/// <summary>
	/// BeforeValidate() method
	/// </summary>
	public required MethodProperties? BeforeValidateMethod { get; init; }

	/// <summary>
	/// AfterValidate() method
	/// </summary>
	public required MethodProperties? AfterValidateMethod { get; init; }
}
