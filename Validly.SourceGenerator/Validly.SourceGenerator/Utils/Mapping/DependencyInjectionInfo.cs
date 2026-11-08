namespace Validly.SourceGenerator.Utils.Mapping;

/// <param name="Name">Simple name of the type; used for the names of the generated variables</param>
/// <param name="TypeFullName">Fully qualified name of the type (global::...)</param>
public record DependencyInjectionInfo(string Name, string TypeFullName);
