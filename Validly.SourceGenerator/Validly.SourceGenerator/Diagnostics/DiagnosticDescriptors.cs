using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Diagnostics;

internal static class DiagnosticDescriptors
{
	private const string Category = "Validly";

	public static readonly DiagnosticDescriptor UnknownJsonNamingPolicy = new(
		id: "VLY0100",
		title: "Unknown JsonNamingPolicy in ValidlyConfiguration",
		messageFormat: "The JsonNamingPolicy value '{0}' in ValidlyConfiguration is not recognized; supported values are: none, camelCase, snake_case_lower, snake_case_upper, kebab_case_lower, kebab_case_upper. The policy is detected from the compilation instead.",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);
}
