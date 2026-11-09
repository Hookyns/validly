using System.Runtime.CompilerServices;

namespace Validly.Validators;

/// <summary>
/// Validator that ensures a value is within enum defined values
/// </summary>
[Validator]
[ValidatorDescription("must be one of the defined enum members")]
[AttributeUsage(AttributeTargets.Property)]
public sealed class InEnumAttribute : Attribute
{
	private static readonly ValidationMessage ValidationMessage = ValidationMessagesHelper.CreateMessage(ValidationCodes.InEnum, LegacyValidationCodes.InEnum, null, "Must be one of the defined enum members."
	);

	/// <summary>
	/// Validate the value
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValidationMessage? IsValid<TEnum>(TEnum value)
		where TEnum : struct, Enum
	{
		return !Enum.IsDefined(typeof(TEnum), value) ? ValidationMessage : null;
	}
}
