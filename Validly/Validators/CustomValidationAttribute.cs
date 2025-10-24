namespace Validly.Validators;

/// <summary>
/// Mark properties with this attribute to indicate that they should be validated with a custom validation method.
/// New interface with validation method will be generated for the property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CustomValidationAttribute : Attribute;
