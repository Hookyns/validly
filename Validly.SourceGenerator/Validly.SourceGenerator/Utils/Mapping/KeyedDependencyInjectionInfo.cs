namespace Validly.SourceGenerator.Utils.Mapping;

public sealed record KeyedDependencyInjectionInfo(string Name, string TypeFullName, string KeySyntax)
	: DependencyInjectionInfo(Name, TypeFullName);
