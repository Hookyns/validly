using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Utils;

public static class Extensions
{
	private static readonly SymbolDisplayFormat QualifiedNameArityFormat = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
	);

	/// <summary>
	/// Filter-out null values
	/// </summary>
	/// <param name="source"></param>
	/// <typeparam name="TSource"></typeparam>
	/// <returns></returns>
	public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(
		this IncrementalValuesProvider<TSource?> source
	)
	{
		return source.Where(static item => item is not null)!;
	}

	/// <summary>
	/// Returns qualified name of the symbol
	/// </summary>
	/// <param name="symbol"></param>
	/// <returns></returns>
	public static string GetQualifiedName(this INamedTypeSymbol symbol)
	{
		return symbol.ToDisplayString(QualifiedNameArityFormat);
	}

	/// <summary>
	/// Indent all lines in the string by given count of tabs
	/// </summary>
	/// <param name="source"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	public static string Indent(this string source, int count = 1)
	{
		return Regex.Replace(source, "^", new string('\t', count), RegexOptions.Multiline);
	}
}
