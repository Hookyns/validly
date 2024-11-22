using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Valigator.SourceGenerator.Dtos;
using Valigator.SourceGenerator.Utils;
using Valigator.SourceGenerator.Utils.Mapping;

namespace Valigator.SourceGenerator.ValueProviders;

internal static class ValidatableObjectIncrementalValueProvider
{
	public static IncrementalValuesProvider<ObjectProperties> Get(IncrementalGeneratorInitializationContext initContext)
	{
		return initContext
			.SyntaxProvider.ForAttributeWithMetadataName(
				Consts.ValidatableAttributeQualifiedName,
				predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
				transform: static (context, cancellationToken) => GetObjectProperties(context, cancellationToken)
			)
			.WhereNotNull();
	}

	private static ObjectProperties? GetObjectProperties(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken
	)
	{
		var symbol = context.TargetSymbol;
		var semanticModel = context.SemanticModel;

		if (symbol is not INamedTypeSymbol typeSymbol || context.TargetNode is not TypeDeclarationSyntax targetNode)
		{
			return null;
		}

		var usings = GetUsings(targetNode);
		var membersSymbols = typeSymbol.GetMembers();
		var methods = GetMethods(membersSymbols, semanticModel);
		var properties = GetProperties(membersSymbols, semanticModel);

		bool inheritsValidatableObject =
			typeSymbol
				.BaseType?.GetAttributes()
				.Any(attr => attr.AttributeClass?.GetQualifiedName() == Consts.ValidatableAttributeQualifiedName)
			?? false;

		var validatableAttribute = typeSymbol
			.GetAttributes()
			.First(attr => attr.AttributeClass?.GetQualifiedName() == Consts.ValidatableAttributeQualifiedName);

		return new ObjectProperties
		{
			UseAutoValidators = GetUseAutoValidatorsValue(validatableAttribute),
			Usings = new EquatableArray<string>(usings.Select(static usingSyntax => usingSyntax.ToString()).ToArray()),
			ClassOrRecordKeyword = typeSymbol.IsRecord ? "record" : "class",
			Accessibility = typeSymbol.DeclaredAccessibility,
			Name = typeSymbol.Name,
			Namespace = typeSymbol.ContainingNamespace.ToString(),
			Properties = new EquatableArray<PropertyProperties>(properties),
			Methods = new EquatableArray<MethodProperties>(methods),
			InheritsValidatableObject = inheritsValidatableObject,
			BeforeValidateMethod = methods.FirstOrDefault(static m => m.MethodName == Consts.BeforeValidateMethodName),
			AfterValidateMethod = methods.FirstOrDefault(static m => m.MethodName == Consts.AfterValidateMethodName),
		};
	}

	private static PropertyProperties[] GetProperties(
		ImmutableArray<ISymbol> membersSymbols,
		SemanticModel semanticModel
	)
	{
		var list = new List<PropertyProperties>();

		foreach (var symbol in membersSymbols)
		{
			if (symbol is not IPropertySymbol propertySymbol)
			{
				continue;
			}

			list.Add(SymbolMapper.MapValidatableProperty(propertySymbol, semanticModel));
		}

		return list.ToArray();
	}

	private static MethodProperties[] GetMethods(ImmutableArray<ISymbol> membersSymbols, SemanticModel semanticModel)
	{
		var list = new List<MethodProperties>();

		foreach (var symbol in membersSymbols)
		{
			if (symbol is not IMethodSymbol methodSymbol)
			{
				continue;
			}

			if (
				methodSymbol.MethodKind
				is MethodKind.Ordinary
					or MethodKind.ExplicitInterfaceImplementation
					or MethodKind.DeclareMethod
			)
			{
				list.Add(SymbolMapper.MapMethod(methodSymbol, semanticModel));
			}
		}

		return list.ToArray();
	}

	private static bool? GetUseAutoValidatorsValue(AttributeData validatableAttribute)
	{
		if (
			validatableAttribute
				.NamedArguments.FirstOrDefault(static x => x.Key == nameof(ValidatableAttribute.UseAutoValidators))
				.Value.Value
			is true
		)
		{
			return true;
		}

		if (
			validatableAttribute
				.NamedArguments.FirstOrDefault(static x => x.Key == nameof(ValidatableAttribute.NoAutoValidators))
				.Value.Value
			is true
		)
		{
			return false;
		}

		return null;
	}

	private static UsingDirectiveSyntax[] GetUsings(TypeDeclarationSyntax targetNode)
	{
		var usings = targetNode.Parent?.Parent is CompilationUnitSyntax cus
			? cus.Usings.ToArray()
			: Array.Empty<UsingDirectiveSyntax>();
		return usings;
	}
}