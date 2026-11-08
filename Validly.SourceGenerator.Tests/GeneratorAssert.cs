using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Tests;

internal static class GeneratorAssert
{
	/// <summary>
	/// Global usings of the SDK's ImplicitUsings; the generated code relies on them
	/// </summary>
	private const string ImplicitUsings = """
		global using System;
		global using System.Collections.Generic;
		global using System.Linq;
		global using System.Threading;
		global using System.Threading.Tasks;

		""";

	/// <summary>
	/// Runs the generator and asserts that neither the generator nor the resulting compilation reports an error
	/// </summary>
	public static GeneratorRunResult CompilesWithoutErrors(string source, string? validlyConfiguration = null)
	{
		GeneratorRunResult result = GeneratorDriverHelper.Run(
			ImplicitUsings + source,
			validlyConfiguration,
			out Compilation compilation
		);

		Assert.Null(result.Exception);
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

		var errors = compilation
			.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Select(d => d.ToString())
			.ToList();

		Assert.True(
			errors.Count == 0,
			"Compilation errors:" + Environment.NewLine + string.Join(Environment.NewLine, errors)
		);

		return result;
	}
}
