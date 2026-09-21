using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Common;

/// <summary>
/// Validator that ensures a value is not empty.
/// A string containing only whitespace characters, a collection without items
/// and a struct equal to its <c>default</c> value are all considered empty.
/// </summary>
[Validator]
[ValidatorDescription("non-empty value required")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotEmptyAttribute : Attribute
{
	private static readonly ValidationMessage NotEmptyMessage = new(
		"A non-empty value is required.",
		"Validly.Validations.NotEmpty"
	);

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public ValidationMessage? IsValid(string? value)
	{
		if (value is not null)
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] is not ('\n' or '\r' or ' ' or '\t'))
				{
					return null;
				}
			}

			return NotEmptyMessage;
		}

		return null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid<T>(ICollection<T>? value)
	{
		if (value is { Count: 0 })
		{
			return NotEmptyMessage;
		}

		return null;
	}

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid<T>(IEnumerable<T>? value)
	{
		if (value is null)
		{
			return null;
		}

		if (value is ICollection<T> collection)
		{
			return IsValid(collection);
		}

		return !value.Any() ? NotEmptyMessage : null;
	}

	/// <summary>
	/// Validate the value. A struct equal to its <c>default</c> value is considered empty.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid<T>(T value)
		where T : struct
	{
		// `default` first; it is the emptiness rule for structs and it also guards struct
		// collections whose members throw when the struct is `default`, e.g. default(ImmutableArray<T>).
		if (EqualityComparer<T>.Default.Equals(value, default))
		{
			return NotEmptyMessage;
		}

		// Struct collections (ImmutableArray<T>, ArraySegment<T>, ...) bind here by identity
		// conversion instead of to the IEnumerable<T> overload; keep the collection semantics.
		if (value is System.Collections.IEnumerable enumerable)
		{
			return IsEnumerableEmpty(enumerable) ? NotEmptyMessage : null;
		}

		return null;
	}

	/// <summary>
	/// Validate the value. <c>null</c> is valid; a non-null struct equal to its <c>default</c>
	/// value is considered empty.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid<T>(T? value)
		where T : struct
	{
		return value.HasValue ? IsValid(value.Value) : null;
	}

	private static bool IsEnumerableEmpty(System.Collections.IEnumerable value)
	{
		if (value is System.Collections.ICollection collection)
		{
			return collection.Count == 0;
		}

		var enumerator = value.GetEnumerator();

		try
		{
			return !enumerator.MoveNext();
		}
		finally
		{
			(enumerator as IDisposable)?.Dispose();
		}
	}
}
