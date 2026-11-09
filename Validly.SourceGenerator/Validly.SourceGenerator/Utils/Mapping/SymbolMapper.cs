using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Validly.SourceGenerator.Utils.Mapping;

internal static class SymbolMapper
{
	public static MethodProperties MapMethod(
		IMethodSymbol methodSymbol,
		SemanticModel? semanticModel,
		bool skipFirstParameter = false,
		bool qualifiedReturnTypeName = false
	)
	{
		DependencyInjectionInfo[] dependencies = new DependencyInjectionInfo[
			skipFirstParameter ? Math.Max(methodSymbol.Parameters.Length - 1, 0) : methodSymbol.Parameters.Length
		];

		int offset = skipFirstParameter ? 1 : 0;

		for (int i = offset; i < methodSymbol.Parameters.Length; i++)
		{
			var parameter = methodSymbol.Parameters[i];
			dependencies[i - offset] = ExtractDependencyInjectionInfo(parameter);
		}

		var namedReturnType = methodSymbol.ReturnType as INamedTypeSymbol;

		return new MethodProperties
		{
			MethodName = GetMethodName(methodSymbol),
			ReturnType =
				methodSymbol.ReturnType.Name == "Void" ? "void"
				: qualifiedReturnTypeName && namedReturnType is not null ? namedReturnType.GetQualifiedName()
				: methodSymbol.ReturnType.Name,
			ReturnTypeType = namedReturnType is not null
				? ToReturnTypeType(namedReturnType, semanticModel)
				: ReturnTypeType.Void,
			ReturnTypeGenericArgument = namedReturnType?.TypeArguments.FirstOrDefault()?.Name,
			Dependencies = new EquatableArray<DependencyInjectionInfo>(dependencies),
			ReturnTypeFullName = methodSymbol.ReturnsVoid ? "void" : ToFullName(methodSymbol.ReturnType),
			IsCalledByBase = OverridesMethodOfValidatableType(methodSymbol),
		};
	}

	private static bool OverridesMethodOfValidatableType(IMethodSymbol methodSymbol)
	{
		for (var overridden = methodSymbol.OverriddenMethod; overridden is not null; overridden = overridden.OverriddenMethod)
		{
			if (
				overridden
					.ContainingType.OriginalDefinition.GetAttributes()
					.Any(static attr => attr.AttributeClass?.GetQualifiedName() == Consts.ValidatableAttributeQualifiedName)
			)
			{
				return true;
			}
		}

		return false;
	}

	private static DependencyInjectionInfo ExtractDependencyInjectionInfo(IParameterSymbol parameter)
	{
		var attributesInfo = parameter.GetAttributes();
		var attributeData = attributesInfo.FirstOrDefault(x =>
			x.AttributeClass?.GetQualifiedName() == Consts.FromKeyedServicesAttributeName
		);

		if (attributeData is not null)
		{
			var constant = attributeData.ConstructorArguments.Single();
			var key = GenerateKeyFromConstant(constant);

			return new KeyedDependencyInjectionInfo(parameter.Type.Name, ToServiceTypeName(parameter.Type), key);
		}

		return new DependencyInjectionInfo(parameter.Type.Name, ToServiceTypeName(parameter.Type));
	}

	private static string GenerateKeyFromConstant(TypedConstant typedConstant)
	{
		var value = typedConstant.Value;

		if (value is null)
		{
			return "null";
		}

		if (typedConstant is { Kind: TypedConstantKind.Enum } enumConstant)
		{
			return $"({(enumConstant.Type is null ? "int" : ToFullName(enumConstant.Type))}) {enumConstant.Value}";
		}

		return SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false) ?? "null";
	}

	private static string GetMethodName(IMethodSymbol methodSymbol)
	{
		if (methodSymbol.MethodKind is MethodKind.ExplicitInterfaceImplementation)
		{
			return methodSymbol.Name.Substring(methodSymbol.Name.LastIndexOf('.') + 1);
		}

		return methodSymbol.Name;
	}

	public static PropertyProperties MapValidatableProperty(IPropertySymbol propertySymbol, SemanticModel? semanticModel)
	{
		var attrs = propertySymbol.GetAttributes();
		var attributes = attrs.Select(MapAttribute).ToArray();

		// Only readable instance properties can be validated as nested objects / collections (this.Property)
		bool isReadableInstanceProperty =
			!propertySymbol.IsStatic && !propertySymbol.IsIndexer && propertySymbol.GetMethod is not null;

		return new PropertyProperties
		{
			PropertyName = propertySymbol.Name,
			DisplayName =
				attrs
					.FirstOrDefault(attr => attr.AttributeClass?.Name == "DisplayNameAttribute")
					?.ConstructorArguments[0]
					.Value?.ToString()
				?? propertySymbol.Name,
			JsonName =
				attrs
					.FirstOrDefault(attr =>
						attr.AttributeClass?.GetQualifiedName() == Consts.JsonPropertyNameAttributeQualifiedName
					)
					?.ConstructorArguments.FirstOrDefault()
					.Value as string,
			PropertyType = propertySymbol.Type.Name,
			PropertyTypeKind = propertySymbol.Type.TypeKind,
			Nullable =
				propertySymbol.NullableAnnotation == NullableAnnotation.Annotated
				|| !IsNullableContextEnabled(propertySymbol, semanticModel),
			PropertyIsOfValidatableType =
				isReadableInstanceProperty
				&& propertySymbol
					.Type.GetAttributes()
					.Any(attr => attr.AttributeClass?.GetQualifiedName() == Consts.ValidatableAttributeQualifiedName),
			Attributes = new EquatableArray<AttributeProperties>(attributes),
			CollectionItemsKind = isReadableInstanceProperty
				? GetCollectionItemsKind(propertySymbol.Type)
				: CollectionItemsKind.None,
			SkipItemsValidation = attrs.Any(attr =>
				attr.AttributeClass?.GetQualifiedName() == Consts.SkipItemsValidationAttributeQualifiedName
			),
		};
	}

	/// <summary>
	/// Nullable context of the symbol's declaration; metadata symbols carry it in their annotations
	/// </summary>
	private static bool IsNullableContextEnabled(ISymbol symbol, SemanticModel? semanticModel)
	{
		var location = symbol.Locations.FirstOrDefault(static l => l.IsInSource);

		if (location?.SourceTree is null || semanticModel is null)
		{
			// Oblivious (no annotation) means the nullable context was disabled
			return symbol is not IPropertySymbol { NullableAnnotation: NullableAnnotation.None };
		}

		// Partial declarations may live in other files
		var model =
			location.SourceTree == semanticModel.SyntaxTree
				? semanticModel
				: semanticModel.Compilation.GetSemanticModel(location.SourceTree);

		return (model.GetNullableContext(location.SourceSpan.Start) & NullableContext.Enabled) == NullableContext.Enabled;
	}

	/// <summary>
	/// Detect a collection of validatable objects
	/// </summary>
	private static CollectionItemsKind GetCollectionItemsKind(ITypeSymbol type)
	{
		if (type is IArrayTypeSymbol arrayType)
		{
			return arrayType.Rank == 1 && IsValidatableType(arrayType.ElementType)
				? CollectionItemsKind.Array
				: CollectionItemsKind.None;
		}

		if (type is not INamedTypeSymbol namedType || type.SpecialType == SpecialType.System_String)
		{
			return CollectionItemsKind.None;
		}

		var enumerableType =
			namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
				? namedType
				: namedType.AllInterfaces.FirstOrDefault(static i =>
					i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
				);

		if (enumerableType is null || !IsValidatableType(enumerableType.TypeArguments[0]))
		{
			return CollectionItemsKind.None;
		}

		return namedType.OriginalDefinition.ToDisplayString() switch
		{
			"System.Collections.Generic.List<T>"
			or "System.Collections.Generic.IList<T>"
			or "System.Collections.Generic.IReadOnlyList<T>" => CollectionItemsKind.List,
			"System.Collections.Immutable.ImmutableArray<T>" => CollectionItemsKind.ImmutableArray,
			_ => CollectionItemsKind.Enumerable,
		};
	}

	private static bool IsValidatableType(ITypeSymbol type)
	{
		return type is INamedTypeSymbol namedType
			&& namedType
				.OriginalDefinition.GetAttributes()
				.Any(static attr => attr.AttributeClass?.GetQualifiedName() == Consts.ValidatableAttributeQualifiedName);
	}

	private static AttributeProperties MapAttribute(AttributeData arg)
	{
		return new AttributeProperties
		{
			QualifiedName = arg.AttributeClass?.GetQualifiedName() ?? string.Empty,
			// TODO: Keep this simple. Move the mapping logic to Generate method; keep this as simple and fast as possible.
			Arguments = new EquatableArray<string>(
				arg.ConstructorArguments.Select(ValueToCode)
					.Concat(
						arg.NamedArguments.Select(x =>
							$"{x.Key} = {x.Value.Value ?? $"[{string.Join(", ", x.Value.Values)}]"}"
						)
					)
					.ToArray()
			),
		};
	}

	/// <summary>
	/// Fully qualified name of the type, usable in any generated file (global::Ns.Type)
	/// </summary>
	public static string ToFullName(ITypeSymbol type)
	{
		return type.ToDisplayString(FullyQualifiedFormat);
	}

	/// <summary>
	/// Service type resolved from the service provider; without the nullable annotation (class constraint)
	/// </summary>
	private static string ToServiceTypeName(ITypeSymbol type)
	{
		return ToFullName(type.WithNullableAnnotation(NullableAnnotation.NotAnnotated));
	}

	private static readonly SymbolDisplayFormat FullyQualifiedFormat =
		SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
			SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
		);

	private static string ValueToCode(TypedConstant constant)
	{
		if (constant.IsNull)
		{
			return "null";
		}

		return constant.Kind switch
		{
			TypedConstantKind.Array =>
				$"new {(constant.Type is IArrayTypeSymbol arrayType ? ToFullName(arrayType.ElementType) : string.Empty)}[] {{ {string.Join(", ", constant.Values.Select(ValueToCode))} }}",
			TypedConstantKind.Type => constant.Value is ITypeSymbol typeSymbol
				? $"typeof({ToFullName(typeSymbol)})"
				: $"typeof({constant.Value})",
			TypedConstantKind.Enum => $"({(constant.Type is null ? "int" : ToFullName(constant.Type))}){constant.Value}",
			TypedConstantKind.Primitive => constant.Value switch
			{
				string s => SymbolDisplay.FormatLiteral(s, quote: true),
				char c => SymbolDisplay.FormatLiteral(c, quote: true),
				bool b => b.ToString().ToLowerInvariant(),
				double d => d.ToString(new NumberFormatInfo { NumberDecimalSeparator = "." }),
				float f => f.ToString(new NumberFormatInfo { NumberDecimalSeparator = "." }),
				_ => constant.Value?.ToString() ?? "null",
			},
			_ => constant.Value?.ToString() ?? "null",
		};
	}

	public static ReturnTypeType ToReturnTypeType(INamedTypeSymbol returnType, SemanticModel? semanticModel)
	{
		ReturnTypeType result = returnType.Name switch
		{
			"Task" => ReturnTypeType.Task,
			"ValueTask" => ReturnTypeType.ValueTask,
			"IAsyncEnumerable" => ReturnTypeType.AsyncEnumerable,
			"IEnumerable" => ReturnTypeType.Enumerable,
			"ValidationResult" => ReturnTypeType.ValidationResult,
			"ValidationMessage" => ReturnTypeType.ValidationMessage,
			"Validation" => ReturnTypeType.Validation,
			_ => ReturnTypeType.Void,
		};

		if ((result & ReturnTypeType.MayBeGeneric) != 0)
		{
			result |= returnType.TypeArguments.FirstOrDefault()?.Name switch
			{
				"ValidationResult" => ReturnTypeType.ValidationResult,
				"ValidationMessage" => ReturnTypeType.ValidationMessage,
				"Validation" => ReturnTypeType.Validation,
				_ => ReturnTypeType.Void,
			};
		}

		if (
			returnType.NullableAnnotation == NullableAnnotation.Annotated
			|| (
				semanticModel is not null
				&& returnType.Locations[0].IsInSource
				&& returnType.Locations[0].SourceTree == semanticModel.SyntaxTree
				&& (
					semanticModel.GetNullableContext(returnType.Locations[0].SourceSpan.Start) & NullableContext.Enabled
				) != NullableContext.Enabled
			)
		)
		{
			result |= ReturnTypeType.Nullable;
		}

		return result;
	}
}
