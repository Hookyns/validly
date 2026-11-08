namespace Validly;

/// <summary>
/// Items of the collection are not validated, even though they are of a validatable type.
/// Rules of the collection property itself (e.g. Required, MinCollectionSize) are still validated.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SkipItemsValidationAttribute : Attribute;
