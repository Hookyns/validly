namespace Validly.SourceGenerator.Utils.Mapping;

public sealed record DependencyInjectionInfo(string Name, bool IsKeyedService, object? Key);