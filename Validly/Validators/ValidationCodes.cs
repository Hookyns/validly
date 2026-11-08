namespace Validly.Validators;

/// <summary>
/// Stable, public codes of the built-in validators: <c>Validations.{Validator}[.{Reason}]</c>
/// </summary>
/// <remarks>
/// Codes are part of the public contract of an API using Validly; they do not change between versions.
/// Validly 1.x codes are available in <see cref="LegacyValidationCodes"/> and used when the
/// <c>Validly.UseLegacyValidationCodes</c> switch is enabled (MSBuild <c>&lt;ValidlyLegacyValidationCodes&gt;true&lt;/ValidlyLegacyValidationCodes&gt;</c>).
/// </remarks>
public static class ValidationCodes
{
	/// <summary>Collection size is not between the minimum and the maximum</summary>
	public const string CollectionSizeBetween = "Validations.CollectionSizeBetween";

	/// <summary>Collection has more items than allowed</summary>
	public const string MaxCollectionSize = "Validations.MaxCollectionSize";

	/// <summary>Collection has fewer items than required</summary>
	public const string MinCollectionSize = "Validations.MinCollectionSize";

	/// <summary>Value is empty</summary>
	public const string NotEmpty = "Validations.NotEmpty";

	/// <summary>Value is missing</summary>
	public const string Required = "Validations.Required";

	/// <summary>Value is not defined by the enum</summary>
	public const string InEnum = "Validations.InEnum";

	/// <summary>Value is not between the minimum and the maximum (inclusive)</summary>
	public const string Between = "Validations.Between";

	/// <summary>Value is not between the minimum and the maximum (exclusive)</summary>
	public const string ExclusiveBetween = "Validations.ExclusiveBetween";

	/// <summary>Value is not greater than the minimum</summary>
	public const string GreaterThan = "Validations.GreaterThan";

	/// <summary>Value is less than the minimum</summary>
	public const string GreaterThanOrEqual = "Validations.GreaterThanOrEqual";

	/// <summary>Value is not less than the maximum</summary>
	public const string LessThan = "Validations.LessThan";

	/// <summary>Value is greater than the maximum</summary>
	public const string LessThanOrEqual = "Validations.LessThanOrEqual";

	/// <summary>Value is not an e-mail address</summary>
	public const string EmailAddress = "Validations.EmailAddress";

	/// <summary>Length is not between the minimum and the maximum</summary>
	public const string LengthBetween = "Validations.LengthBetween";

	/// <summary>Value does not match the pattern</summary>
	public const string Match = "Validations.Match";

	/// <summary>Value is longer than allowed</summary>
	public const string MaxLength = "Validations.MaxLength";

	/// <summary>Value is shorter than required</summary>
	public const string MinLength = "Validations.MinLength";

	/// <summary>Password is shorter than required</summary>
	public const string PasswordLength = "Validations.Password.Length";

	/// <summary>Password does not contain enough special characters</summary>
	public const string PasswordSpecialChar = "Validations.Password.SpecialChar";

	/// <summary>Password does not contain enough upper-case characters</summary>
	public const string PasswordUpperCase = "Validations.Password.UpperCase";

	/// <summary>Password does not contain enough digits</summary>
	public const string PasswordDigit = "Validations.Password.Digit";
}

/// <summary>
/// Codes (resource keys) of the built-in validators used by Validly 1.x
/// </summary>
public static class LegacyValidationCodes
{
	/// <summary>Validly 1.x code of <see cref="ValidationCodes.CollectionSizeBetween"/></summary>
	public const string CollectionSizeBetween = "Validly.Validations.CollectionSizeBetween";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.MaxCollectionSize"/></summary>
	public const string MaxCollectionSize = "Validly.Validations.MaxCollectionSize";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.MinCollectionSize"/></summary>
	public const string MinCollectionSize = "Validly.Validations.MinCollectionSize";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.NotEmpty"/></summary>
	public const string NotEmpty = "Validly.Validations.NotEmpty";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.Required"/></summary>
	public const string Required = "Validly.Validations.Required";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.InEnum"/></summary>
	public const string InEnum = "Validly.Validations.InEnum";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.Between"/></summary>
	public const string Between = "Validly.Validations.Between.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.ExclusiveBetween"/></summary>
	public const string ExclusiveBetween = "Validly.Validations.ExclusiveBetween.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.GreaterThan"/></summary>
	public const string GreaterThan = "Validly.Validations.GreaterThan.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.GreaterThanOrEqual"/></summary>
	public const string GreaterThanOrEqual = "Validly.Validations.GreaterThanOrEqual.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.LessThan"/></summary>
	public const string LessThan = "Validly.Validations.LessThan.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.LessThanOrEqual"/></summary>
	public const string LessThanOrEqual = "Validly.Validations.LessThanOrEqual.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.EmailAddress"/></summary>
	public const string EmailAddress = "Validly.Validations.EmailAddress.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.LengthBetween"/></summary>
	public const string LengthBetween = "Validly.Validations.LengthBetween.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.Match"/></summary>
	public const string Match = "Validly.Validations.Match.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.MaxLength"/></summary>
	public const string MaxLength = "Validly.Validations.MaxLength.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.MinLength"/></summary>
	public const string MinLength = "Validly.Validations.MinLength.Error";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.PasswordLength"/></summary>
	public const string PasswordLength = "Validly.Validations.Password.Error.Length";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.PasswordSpecialChar"/></summary>
	public const string PasswordSpecialChar = "Validly.Validations.Password.Error.SpecialChar";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.PasswordUpperCase"/></summary>
	public const string PasswordUpperCase = "Validly.Validations.Password.Error.UpperCase";

	/// <summary>Validly 1.x code of <see cref="ValidationCodes.PasswordDigit"/></summary>
	public const string PasswordDigit = "Validly.Validations.Password.Error.Digit";
}
