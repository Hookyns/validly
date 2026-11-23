namespace Validly.Naming;

/// <summary>
/// Naming policy used for the property names in the validation problem details
/// </summary>
/// <remarks>
/// Built-in policies are converted without System.Text.Json; a custom policy is invoked through the delegate.
/// </remarks>
public sealed class PropertyNamingPolicy
{
	/// <summary>
	/// camelCase
	/// </summary>
	public static PropertyNamingPolicy CamelCase { get; } = new(ValidlyNamingPolicy.CamelCase);

	/// <summary>
	/// snake_case
	/// </summary>
	public static PropertyNamingPolicy SnakeCaseLower { get; } = new(ValidlyNamingPolicy.SnakeCaseLower);

	/// <summary>
	/// SNAKE_CASE
	/// </summary>
	public static PropertyNamingPolicy SnakeCaseUpper { get; } = new(ValidlyNamingPolicy.SnakeCaseUpper);

	/// <summary>
	/// kebab-case
	/// </summary>
	public static PropertyNamingPolicy KebabCaseLower { get; } = new(ValidlyNamingPolicy.KebabCaseLower);

	/// <summary>
	/// KEBAB-CASE
	/// </summary>
	public static PropertyNamingPolicy KebabCaseUpper { get; } = new(ValidlyNamingPolicy.KebabCaseUpper);

	private readonly Func<string, string>? _convertName;

	/// <summary>
	/// Built-in policy represented by this instance; <see cref="ValidlyNamingPolicy.None"/> for a custom policy
	/// </summary>
	public ValidlyNamingPolicy Policy { get; }

	private PropertyNamingPolicy(ValidlyNamingPolicy policy)
	{
		Policy = policy;
	}

	private PropertyNamingPolicy(Func<string, string> convertName)
	{
		_convertName = convertName;
	}

	/// <summary>
	/// Returns the policy for the given built-in policy; null for <see cref="ValidlyNamingPolicy.None"/>
	/// </summary>
	/// <param name="policy"></param>
	/// <returns></returns>
	public static PropertyNamingPolicy? FromPolicy(ValidlyNamingPolicy policy)
	{
		return policy switch
		{
			ValidlyNamingPolicy.CamelCase => CamelCase,
			ValidlyNamingPolicy.SnakeCaseLower => SnakeCaseLower,
			ValidlyNamingPolicy.SnakeCaseUpper => SnakeCaseUpper,
			ValidlyNamingPolicy.KebabCaseLower => KebabCaseLower,
			ValidlyNamingPolicy.KebabCaseUpper => KebabCaseUpper,
			_ => null,
		};
	}

	/// <summary>
	/// Creates a policy converting the names with the given delegate
	/// </summary>
	/// <param name="convertName"></param>
	/// <returns></returns>
	public static PropertyNamingPolicy Create(Func<string, string> convertName)
	{
		return new PropertyNamingPolicy(convertName ?? throw new ArgumentNullException(nameof(convertName)));
	}

	/// <summary>
	/// Converts the property name according to this policy
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public string ConvertName(string name)
	{
		return _convertName is not null ? _convertName(name) : NamingPolicyConverter.Convert(Policy, name);
	}
}
