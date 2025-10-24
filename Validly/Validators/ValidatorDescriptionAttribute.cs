using System.Diagnostics.CodeAnalysis;

namespace Validly.Validators;

/// <summary>
/// Description of a validator
/// </summary>
/// <param name="description">Composite format string for the description. Arguments are the constructor parameters.</param>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class)]
public sealed class ValidatorDescriptionAttribute(
	[StringSyntax(StringSyntaxAttribute.CompositeFormat)] string description
) : Attribute
{
	/// <summary>
	/// Composite format string for the description
	/// </summary>
	/// <remarks>
	/// Arguments are the constructor parameters.
	/// </remarks>
	public string Description { get; } = description;
}
