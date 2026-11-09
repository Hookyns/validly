using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Validly;

/// <summary>
/// Validation of objects by their runtime type. Detached validatable objects
/// (<see cref="ValidatableAttribute.Detached"/>) do not implement <see cref="IValidatable"/>; they are registered
/// here (by a generated module initializer), so e.g. the ASP.NET Core filter can validate them.
/// </summary>
public static class ValidatorRegistry
{
	/// <summary>
	/// Validates the object (of the registered type)
	/// </summary>
	public delegate ValueTask<ValidationResult> ValidateDelegate(
		object obj,
		IServiceProvider? serviceProvider,
		CancellationToken cancellationToken
	);

	private static readonly ConcurrentDictionary<Type, ValidateDelegate> Validators = new();

	/// <summary>
	/// Register the validation of the type
	/// </summary>
	public static void Register<T>(Func<T, IServiceProvider?, CancellationToken, ValueTask<ValidationResult>> validate)
		where T : class
	{
		Validators[typeof(T)] = (obj, serviceProvider, cancellationToken) =>
			validate((T)obj, serviceProvider, cancellationToken);
	}

	/// <summary>
	/// Get the validation registered for the type
	/// </summary>
	public static bool TryGetValidator(Type type, [NotNullWhen(true)] out ValidateDelegate? validate)
	{
		return Validators.TryGetValue(type, out validate);
	}

	/// <summary>
	/// Validate the object; through <see cref="IValidatable"/> or the registered validation of its type
	/// </summary>
	/// <returns>False when the object is not validatable</returns>
	public static bool TryValidate(
		object obj,
		IServiceProvider? serviceProvider,
		CancellationToken cancellationToken,
		out ValueTask<ValidationResult> result
	)
	{
		if (obj is IValidatable validatable)
		{
			result = validatable.ValidateAsync(serviceProvider!, cancellationToken);
			return true;
		}

		if (Validators.TryGetValue(obj.GetType(), out var validate))
		{
			result = validate(obj, serviceProvider, cancellationToken);
			return true;
		}

		result = default;
		return false;
	}
}
