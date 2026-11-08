using Validly.Naming;
using Validly.SourceGenerator.Utils.Mapping;

namespace Validly.SourceGenerator.Utils;

/// <summary>
/// Resolves the JSON name of a property at compile time
/// </summary>
internal static class JsonNameResolver
{
	/// <summary>
	/// Returns the JSON name to generate, or null when the name has to be resolved at runtime
	/// </summary>
	/// <remarks>
	/// [JsonPropertyName] wins; otherwise the known naming policy is applied to the property name.
	/// Without a known policy there is nothing to generate and the runtime applies the policy it is given.
	/// </remarks>
	/// <param name="property"></param>
	/// <param name="namingPolicy"></param>
	/// <returns></returns>
	public static string? Resolve(PropertyProperties property, ValidlyNamingPolicy namingPolicy)
	{
		if (property.JsonName is not null)
		{
			return property.JsonName;
		}

		return namingPolicy == ValidlyNamingPolicy.None
			? null
			: NamingPolicyConverter.Convert(namingPolicy, property.PropertyName);
	}
}
