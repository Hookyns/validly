namespace Validly.Utils;

/// <summary>
/// Immutable segment of a property path; linked from the root to the leaf
/// </summary>
/// <param name="Name">CLR name of the property</param>
/// <param name="JsonName">Explicit JSON name of the property (from [JsonPropertyName])</param>
/// <param name="Next">Next (child) segment</param>
internal sealed record PropertyPathSegment(string Name, string? JsonName, PropertyPathSegment? Next);
