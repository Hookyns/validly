using System.Runtime.CompilerServices;
using Validly.Validators;

namespace Validly.Extensions.Validators.Common;

/// <summary>
/// Validator that ensures a value is a non-empty string
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
}
