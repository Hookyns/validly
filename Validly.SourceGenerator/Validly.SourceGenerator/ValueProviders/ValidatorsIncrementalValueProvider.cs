using Microsoft.CodeAnalysis;
using Validly.SourceGenerator.Dtos;
using Validly.SourceGenerator.Utils;
using Validly.SourceGenerator.Utils.Mapping;
using Validly.SourceGenerator.Utils.Symbols;

namespace Validly.SourceGenerator.ValueProviders;

internal static class ValidatorsIncrementalValueProvider
{
	public static IncrementalValueProvider<EquatableArray<ValidatorProperties>> Get(
		IncrementalGeneratorInitializationContext initContext
	)
	{
		var referencedValidators = initContext
			.CompilationProvider.SelectMany(
				(compilation, cancellationToken) =>
					compilation.SourceModule.ReferencedAssemblySymbols.SelectMany(assemblySymbol =>
						SelectValidatorsProperties(assemblySymbol, cancellationToken)
					)
			)
			.Collect();

		var projectValidators = initContext.CompilationProvider.Select(
			(compilation, cancellationToken) =>
				SelectValidatorsProperties(compilation.SourceModule.ContainingAssembly, cancellationToken)
		);

		var allValidators = referencedValidators
			.Combine(projectValidators)
			.Select((tuple, _) => new EquatableArray<ValidatorProperties>(tuple.Left.Concat(tuple.Right).ToArray()));

		return allValidators;
	}

	private static IEnumerable<ValidatorProperties> SelectValidatorsProperties(
		IAssemblySymbol assemblySymbol,
		CancellationToken cancellationToken
	)
	{
		return CustomSymbolVisitor
			.GetNamedTypes(
				assemblySymbol,
				cancellationToken,
				t =>
					t.GetAttributes()
						.Any(attr => attr.AttributeClass?.GetQualifiedName() == Consts.ValidatorAttributeQualifiedName)
			)
			.Select(GetValidatorProperties);
	}

	private static ValidatorProperties GetValidatorProperties(INamedTypeSymbol typeSymbol)
	{
		IMethodSymbol? isValidMethod =
			typeSymbol.GetMembers(Consts.IsValidMethodName).FirstOrDefault() as IMethodSymbol;

		return new ValidatorProperties
		{
			QualifiedName = typeSymbol.GetQualifiedName(),
			IsValidMethod = isValidMethod is null
				? EmptyIsValidMethodProperties
				// Skip first; it should always be `object?` - the validated value
				: SymbolMapper.MapMethod(isValidMethod, null, qualifiedReturnTypeName: true, skipFirstParameter: true),
		};
	}

	private static readonly MethodProperties EmptyIsValidMethodProperties =
		new()
		{
			MethodName = Consts.IsValidMethodName,
			ReturnType = "void",
			ReturnTypeGenericArgument = null,
			ReturnTypeType = ReturnTypeType.Void,
			Dependencies = new EquatableArray<DependencyInjectionInfo>(),
		};
}
