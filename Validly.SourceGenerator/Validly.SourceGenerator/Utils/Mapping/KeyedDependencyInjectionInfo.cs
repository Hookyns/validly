namespace Validly.SourceGenerator.Utils.Mapping;

internal sealed record KeyedDependencyInjectionInfo(string Name, string TypeFullName, string KeySyntax)
	: DependencyInjectionInfo(Name, TypeFullName);
