using Microsoft.Extensions.DependencyInjection;

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
		return service as T ??
		       throw new InvalidOperationException($"There is no registered service {typeof(T).FullName}");
	}

	/// <summary>
	/// Get the service of type T from the service provider keyed by the specified key
	/// </summary>
	/// <typeparam name="T">Type of the service to retrieve</typeparam>
	/// <param name="provider">The service provider to retrieve the service from</param>
	/// <param name="serviceKey">A key used to identify the service</param>
	/// <returns>The requested service of type T</returns>
	/// <exception cref="ArgumentNullException">Thrown if the provider is null</exception>
	/// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
	public static T GetRequiredKeyedService<T>(IServiceProvider? provider, object serviceKey)
		where T : class
	{
		switch (provider)
		{
			case null:
				throw new ArgumentNullException(nameof(provider), "IServiceProvider can't be null.");
			case IKeyedServiceProvider keyedServiceProvider:
			{
				var service = keyedServiceProvider.GetRequiredKeyedService(typeof(T), serviceKey);
				return service as T ??
				       throw new InvalidOperationException(
					       $"There is no registered keyed service {typeof(T).FullName} with key '{serviceKey}'");
			}
			default:
				throw new InvalidOperationException("The provided IServiceProvider does not support keyed services.");
		}
	}
}