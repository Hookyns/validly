using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Validly.SourceGenerator.Utils.Symbols;

internal class CustomSymbolVisitor
{
	public static ImmutableArray<INamedTypeSymbol> GetNamedTypes(
		IAssemblySymbol assemblySymbol,
		CancellationToken cancellationToken = default,
		Func<INamedTypeSymbol, bool>? predicate = null
	)
	{
		// Skip assemblies that don't reference Validly; Validly itself contains the built-in validators
		if (
			assemblySymbol.Name != "Validly"
			&& (
				!assemblySymbol.GlobalNamespace.ContainingModule?.ReferencedAssemblySymbols.Any(refAssembly =>
					refAssembly.Name == "Validly"
				) ?? true
			)
		)
		{
			return ImmutableArray<INamedTypeSymbol>.Empty;
		}

		return GetNamedTypes(assemblySymbol.GlobalNamespace, cancellationToken, predicate);
	}

	private static ImmutableArray<INamedTypeSymbol> GetNamedTypes(
		INamespaceSymbol namespaceSymbol,
		CancellationToken cancellationToken,
		Func<INamedTypeSymbol, bool>? predicate
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		// Exclude System and Microsoft namespaces
		if (namespaceSymbol.Name.StartsWith("System") || namespaceSymbol.Name.StartsWith("Microsoft"))
		{
			return ImmutableArray<INamedTypeSymbol>.Empty;
		}

		IEnumerable<INamedTypeSymbol> namespaceTypes = namespaceSymbol.GetTypeMembers();

		namespaceTypes = predicate is not null
			? namespaceTypes.Where(namedTypeSymbol =>
				namedTypeSymbol.Kind == SymbolKind.NamedType && predicate(namedTypeSymbol)
			)
			: namespaceTypes.Where(namedTypeSymbol => namedTypeSymbol.Kind == SymbolKind.NamedType);

		return namespaceTypes
			.Concat(
				namespaceSymbol.GetNamespaceMembers().SelectMany(ns => GetNamedTypes(ns, cancellationToken, predicate))
			)
			.ToImmutableArray();
	}
}
