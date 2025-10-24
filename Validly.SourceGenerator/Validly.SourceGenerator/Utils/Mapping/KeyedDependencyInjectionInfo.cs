namespace Validly.SourceGenerator.Utils.Mapping;

public sealed record KeyedDependencyInjectionInfo(string Name, string KeySyntax) : DependencyInjectionInfo(Name);
