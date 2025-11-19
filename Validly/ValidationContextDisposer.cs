namespace Validly;

/// <summary>
/// Conditional disposer of ValidationContext
/// </summary>
public sealed class ValidationContextDisposer : IDisposable
{
	private readonly ValidationContext _validationContext;
	private readonly bool _dispose;

	/// <param name="validationContext"></param>
	/// <param name="dispose">Set true if the ValidationContext should be disposed.</param>
	public ValidationContextDisposer(ValidationContext validationContext, bool dispose)
	{
		_validationContext = validationContext;
		_dispose = dispose;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_dispose)
		{
			_validationContext.Dispose();
		}
	}
}
