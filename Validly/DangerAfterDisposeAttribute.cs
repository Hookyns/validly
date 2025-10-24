namespace Validly;

/// <summary>
/// Marks a method as dangerous to call after the object has been disposed
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DangerAfterDisposeAttribute : Attribute;
