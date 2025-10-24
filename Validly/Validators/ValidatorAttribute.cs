namespace Validly.Validators;

/// <summary>
/// Marks a class as a validator
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ValidatorAttribute : Attribute;
