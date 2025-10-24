using Microsoft.Extensions.ObjectPool;

namespace Validly;

/// <summary>
/// Validation context with information about the object being validated
/// </summary>
public sealed record ValidationContext : IDisposable
{
	private static readonly ObjectPool<ValidationContext> Pool = ObjectPool.Create<ValidationContext>();

	private object _rootObject = null!;
	private object _object = null!;
	private string _propertyName = string.Empty;
	private string _propertyDisplayName = string.Empty;

	/// <summary>
	/// Root object that is being validated
	/// </summary>
	public object RootObject => _rootObject;

	/// <summary>
	/// Current object that is being validated. Is child of the root object
	/// </summary>
	public object Object => _object;

	/// <summary>
	/// Name of current property that is being validated
	/// </summary>
	public string PropertyName => _propertyName;

	/// <summary>
	/// DisplayName of current property that is being validated
	/// </summary>
	public string PropertyDisplayName => _propertyDisplayName;

	/// <summary>
	/// Create new validation context
	/// </summary>
	/// <param name="rootObject"></param>
	/// <returns></returns>
	public static ValidationContext Create(object rootObject)
	{
		var c = Pool.Get();
		c._rootObject = rootObject;
		c._object = rootObject;
		return c;
	}

	/// <summary>
	/// Change the current object that is being validated.
	/// </summary>
	/// <param name="obj"></param>
	public void SetObject(object obj)
	{
		_object = obj;
	}

	/// <summary>
	/// Change the current property that is being validated.
	/// </summary>
	/// <param name="propertyName"></param>
	public void SetProperty(string propertyName)
	{
		_propertyName = propertyName;
		_propertyDisplayName = propertyName;
	}

	/// <summary>
	/// Change the current property that is being validated.
	/// </summary>
	/// <param name="propertyName"></param>
	/// <param name="displayName"></param>
	public void SetProperty(string propertyName, string displayName)
	{
		_propertyName = propertyName;
		_propertyDisplayName = displayName;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Pool.Return(this);
	}
}
