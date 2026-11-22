using System.Text.Json;
using Validly.Naming;

namespace Validly.Extensions.AspNetCore;

/// <summary>
/// Maps the configured System.Text.Json naming policy to the Validly one
/// </summary>
internal static class JsonNamingPolicyMapper
{
	/// <summary>
	/// The last mapped custom policy; an application uses one policy, so it is wrapped just once
	/// </summary>
	private static CustomPolicy? _customPolicy;

	/// <summary>
	/// Returns the matching Validly policy; a custom policy is wrapped, so it is invoked through its ConvertName
	/// </summary>
	/// <param name="namingPolicy"></param>
	/// <returns></returns>
	public static PropertyNamingPolicy? Map(JsonNamingPolicy? namingPolicy)
	{
		if (namingPolicy is null)
		{
			return null;
		}

		// The built-in policies are singletons, so they can be compared by reference
		if (ReferenceEquals(namingPolicy, JsonNamingPolicy.CamelCase))
		{
			return PropertyNamingPolicy.CamelCase;
		}

		if (ReferenceEquals(namingPolicy, JsonNamingPolicy.SnakeCaseLower))
		{
			return PropertyNamingPolicy.SnakeCaseLower;
		}

		if (ReferenceEquals(namingPolicy, JsonNamingPolicy.SnakeCaseUpper))
		{
			return PropertyNamingPolicy.SnakeCaseUpper;
		}

		if (ReferenceEquals(namingPolicy, JsonNamingPolicy.KebabCaseLower))
		{
			return PropertyNamingPolicy.KebabCaseLower;
		}

		if (ReferenceEquals(namingPolicy, JsonNamingPolicy.KebabCaseUpper))
		{
			return PropertyNamingPolicy.KebabCaseUpper;
		}

		CustomPolicy? customPolicy = _customPolicy;

		if (customPolicy is null || !ReferenceEquals(customPolicy.Source, namingPolicy))
		{
			_customPolicy = customPolicy = new CustomPolicy(
				namingPolicy,
				PropertyNamingPolicy.Create(namingPolicy.ConvertName)
			);
		}

		return customPolicy.Mapped;
	}

	private sealed record CustomPolicy(JsonNamingPolicy Source, PropertyNamingPolicy Mapped);
}
