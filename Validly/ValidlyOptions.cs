namespace Validly;

/// <summary>
/// Configuration
/// </summary>
public static class ValidlyOptions
{
	/// <summary>
	/// Size of the ArrayPool for <see cref="ValidationResult.Global"/> message collection
	/// </summary>
	public static int GlobalMessagesPoolSize { get; internal set; } = 16;

	/// <summary>
	/// Size of the ArrayPool for messages in the <see cref="PropertyValidationResult"/>
	/// </summary>
	public static int PropertyMessagesPoolSize { get; internal set; } = 16;

	/// <summary>
	/// Max number of items of <see cref="Validly.Utils.FinalizableObjectPool{TItem}"/>
	/// </summary>
	public static int ObjectPoolSize { get; internal set; } = Environment.ProcessorCount * 2;

	/// <summary>
	/// Name of the AppContext switch enabling the Validly 1.x codes (<see cref="Validly.Validators.LegacyValidationCodes"/>)
	/// </summary>
	public const string UseLegacyValidationCodesSwitchName = "Validly.UseLegacyValidationCodes";

	/// <summary>
	/// The built-in validators use the Validly 1.x codes (<see cref="Validly.Validators.LegacyValidationCodes"/>) and the
	/// problem details JSON contains "resourceKey" instead of "code".
	/// </summary>
	/// <remarks>
	/// Process-wide; read once. Enable it by the MSBuild property <c>&lt;ValidlyLegacyValidationCodes&gt;true&lt;/ValidlyLegacyValidationCodes&gt;</c>,
	/// or <c>AppContext.SetSwitch("Validly.UseLegacyValidationCodes", true)</c> before the first validation.
	/// </remarks>
	public static bool UseLegacyValidationCodes { get; } =
		AppContext.TryGetSwitch(UseLegacyValidationCodesSwitchName, out bool isEnabled) && isEnabled;

	/// <summary>
	/// Configure Validly
	/// </summary>
	public static void Configure(Action<OptionsBuilder> configure)
	{
		configure(new OptionsBuilder());
	}
}

/// <summary>
/// Validator's Options builder
/// </summary>
public class OptionsBuilder
{
	/// <summary>
	/// Set size of the ArrayPool for <see cref="ValidationResult.Global"/> in <see cref="ValidationResult"/>
	/// </summary>
	/// <param name="size"></param>
	/// <returns></returns>
	public OptionsBuilder WithGlobalMessagesPoolSize(int size)
	{
		ValidlyOptions.GlobalMessagesPoolSize = size;
		return this;
	}

	/// <summary>
	/// Set size of the ArrayPool for messages in the <see cref="PropertyValidationResult"/>
	/// </summary>
	/// <param name="size"></param>
	/// <returns></returns>
	public OptionsBuilder WithPropertyMessagesPoolSize(int size)
	{
		ValidlyOptions.PropertyMessagesPoolSize = size;
		return this;
	}

	/// <summary>
	/// Set max number of items of <see cref="Validly.Utils.FinalizableObjectPool{TItem}"/>
	/// </summary>
	/// <param name="size"></param>
	/// <returns></returns>
	public OptionsBuilder WithObjectPoolSize(int size)
	{
		ValidlyOptions.ObjectPoolSize = size;
		return this;
	}
}
