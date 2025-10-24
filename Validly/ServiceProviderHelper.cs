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
	public static TService GetRequiredService<TService>(IServiceProvider? provider)
		where TService : class
	{
		return provider!.GetRequiredService<TService>();
	}

	/// <summary>
	/// Get the service of type T from the service provider keyed by the specified key
	/// </summary>
	/// <typeparam name="TService">Type of the service to retrieve</typeparam>
	/// <param name="serviceProvider">The service provider to retrieve the service from</param>
	/// <param name="serviceKey">A key used to identify the service</param>
	/// <returns>The requested service of type T</returns>
	/// <exception cref="ArgumentNullException">Thrown if the provider is null</exception>
	/// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
	public static TService GetRequiredKeyedService<TService>(IServiceProvider? serviceProvider, object serviceKey)
		where TService : class
	{
		return serviceProvider!.GetRequiredKeyedService<TService>(serviceKey);
	}
}
