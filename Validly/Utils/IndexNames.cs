using System.Globalization;

namespace Validly.Utils;

/// <summary>
/// Cached string representations of collection indexes used in property paths
/// </summary>
internal static class IndexNames
{
	private const int CachedCount = 256;

	private static readonly string[] Cache = CreateCache();

	public static string Get(int index)
	{
		return (uint)index < CachedCount ? Cache[index] : index.ToString(CultureInfo.InvariantCulture);
	}

	private static string[] CreateCache()
	{
		var cache = new string[CachedCount];

		for (int index = 0; index < cache.Length; index++)
		{
			cache[index] = index.ToString(CultureInfo.InvariantCulture);
		}

		return cache;
	}
}
