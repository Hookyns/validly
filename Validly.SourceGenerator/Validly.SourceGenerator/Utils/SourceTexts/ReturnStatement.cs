namespace Validly.SourceGenerator.Utils.SourceTexts;

/// <summary>
/// Return statements of the generated ValidateAsync method.
/// </summary>
/// <remarks>
/// The generated method returns <c>ValueTask&lt;ValidationResult&gt;</c>; when it is not <c>async</c>,
/// the returned value must be wrapped in the ValueTask.
/// </remarks>
internal static class ReturnStatement
{
	/// <summary>
	/// Return statement returning the given ValidationResult expression
	/// </summary>
	/// <param name="validationResultExpression">Expression of type ValidationResult</param>
	/// <param name="isAsync">Whether the generated method is async</param>
	/// <returns></returns>
	public static string Create(string validationResultExpression, bool isAsync)
	{
		return isAsync
			? $"return {validationResultExpression};"
			: $"return new ValueTask<{Consts.ValidationResultGlobalRef}>({validationResultExpression});";
	}

	/// <summary>
	/// Return statement returning the validation result being built
	/// </summary>
	/// <param name="isAsync">Whether the generated method is async</param>
	/// <returns></returns>
	public static string CreateForResult(bool isAsync)
	{
		return Create($"({Consts.ValidationResultGlobalRef})result", isAsync);
	}
}
