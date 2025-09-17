namespace Validly;

/// <summary>
/// Static class for validation dependency extraction
/// </summary>
public static class ServiceProviderHelper
{
	/// <summary>
	/// Get the service of type T from service provider
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown if provider is null</exception>
	/// <exception cref="InvalidOperationException">Thrown if service is not registered</exception>
	public static T GetRequiredService<T>(IServiceProvider? provider)
		where T : class
	{
		if (provider is null)
			throw new ArgumentNullException(nameof(provider), "IServiceProvider can't be null.");
		var service = provider.GetService(typeof(T));
		return service as T ?? throw new InvalidOperationException($"There is no registered service {typeof(T).FullName}");
	}
}