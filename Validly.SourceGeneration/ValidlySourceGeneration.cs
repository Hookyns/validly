using Microsoft.CodeAnalysis;
using Validly.SourceGenerator;
using Validly.SourceGenerator.Dtos;
using Validly.SourceGenerator.Utils;
using Validly.SourceGenerator.Utils.Mapping;
using Validly.SourceGenerator.ValueProviders;

namespace Validly.SourceGeneration;

/// <summary>
/// Generates Validly validation source code from third-party source generators.
/// </summary>
/// <example>
/// <code>
/// var validators = ValidlySourceGeneration.GetValidators(context); // in Initialize()
/// ...
/// var result = ValidlySourceGeneration.Generate(entitySymbol, compilation, new ValidlyGenerationOptions
/// {
///     TypeName = "UserDto",
///     Namespace = "My.Dtos",
/// }, validators);
/// productionContext.AddSource(result.HintName, result.SourceText);
/// </code>
/// The target type (UserDto) must be a partial class/record with properties of the same names (or mapped by
/// <see cref="ValidlyGenerationOptions.PropertyMap"/>); it does not need the [Validatable] attribute.
/// The consuming project must reference the Validly package (the generated code uses its runtime).
/// </example>
public static class ValidlySourceGeneration
{
	/// <summary>
	/// Collect the validators (built-in and custom) available to the compilation. It scans the referenced
	/// assemblies, so reuse the result; in incremental generators prefer
	/// <see cref="GetValidators(IncrementalGeneratorInitializationContext)"/>.
	/// </summary>
	public static ValidlyValidatorCatalog GetValidators(Compilation compilation, CancellationToken cancellationToken = default)
	{
		return new ValidlyValidatorCatalog(ValidatorsIncrementalValueProvider.GetValidators(compilation, cancellationToken));
	}

	/// <summary>
	/// Incremental provider of the validators available to the compilation; the catalog is equatable, so the
	/// downstream steps are cached when the validators do not change.
	/// </summary>
	public static IncrementalValueProvider<ValidlyValidatorCatalog> GetValidators(
		IncrementalGeneratorInitializationContext context
	)
	{
		return context.CompilationProvider.Select(static (compilation, cancellationToken) =>
			GetValidators(compilation, cancellationToken)
		);
	}

	/// <summary>
	/// Generate the validation source of the source type (its validation attributes and hooks), emitted as
	/// a partial part of the target type described by the options.
	/// </summary>
	/// <param name="sourceType">Type with the validation rules, declared in source or in a referenced assembly</param>
	/// <param name="compilation">Compilation containing (or referencing) the type</param>
	/// <param name="options">Target type and what is copied; defaults generate the validation of the type itself</param>
	/// <param name="validators">Validators catalog; collected from the compilation when not provided</param>
	/// <param name="cancellationToken"></param>
	public static ValidlyGenerationResult Generate(
		INamedTypeSymbol sourceType,
		Compilation compilation,
		ValidlyGenerationOptions? options = null,
		ValidlyValidatorCatalog? validators = null,
		CancellationToken cancellationToken = default
	)
	{
		options ??= new ValidlyGenerationOptions();
		validators ??= GetValidators(compilation, cancellationToken);

		ObjectProperties model = MapModel(sourceType, compilation, options, cancellationToken);
		var configuration = new ValidlyConfiguration
		{
			AutoRequired = options.AutoRequired,
			AutoInEnum = options.AutoInEnum,
			ExitEarly = options.ExitEarly,
			ValidateCollectionItems = options.ValidateCollectionItems,
			JsonNamingPolicy = options.JsonNamingPolicy is null
				? null
				: ConfigOptionsProvider.ToNamingPolicy(options.JsonNamingPolicy),
		};

		var (hintName, sourceText) = ValidatableSourceGenerator.GenerateSource((model, configuration, validators.Validators));

		return new ValidlyGenerationResult(hintName, sourceText);
	}

	private static ObjectProperties MapModel(
		INamedTypeSymbol sourceType,
		Compilation compilation,
		ValidlyGenerationOptions options,
		CancellationToken cancellationToken
	)
	{
		ObjectProperties model = MapObject(sourceType, compilation, cancellationToken);
		var properties = model.Properties.GetArray() ?? Array.Empty<PropertyProperties>();
		BaseObjectProperties? baseObject = model.BaseObject;

		// Properties of the ancestors become properties of the target; base properties go first
		if (options.IncludeBaseProperties)
		{
			var allProperties = new List<PropertyProperties>();
			var names = new HashSet<string>(properties.Select(static p => p.PropertyName));
			var ancestors = new List<PropertyProperties[]>();

			for (var baseType = sourceType.BaseType; baseType is { SpecialType: not SpecialType.System_Object }; baseType = baseType.BaseType)
			{
				var baseProperties = MapObject(baseType, compilation, cancellationToken).Properties.GetArray();

				if (baseProperties is not null)
				{
					ancestors.Insert(0, baseProperties.Where(p => names.Add(p.PropertyName)).ToArray());
				}
			}

			foreach (var ancestorProperties in ancestors)
			{
				allProperties.AddRange(ancestorProperties);
			}

			allProperties.AddRange(properties);
			properties = allProperties.ToArray();
			baseObject = null;
		}

		// Renamed / excluded properties
		var mappedProperties = new List<PropertyProperties>(properties.Length);

		foreach (var property in properties)
		{
			string? targetName = options.PropertyMap is null ? property.PropertyName : options.PropertyMap(property.PropertyName);

			if (targetName is null)
			{
				continue;
			}

			var mapped = property with
			{
				PropertyName = targetName,
				DisplayName = property.DisplayName == property.PropertyName ? targetName : property.DisplayName,
			};

			if (!options.CopyCustomValidation)
			{
				mapped = mapped with
				{
					Attributes = new EquatableArray<AttributeProperties>(
						(mapped.Attributes.GetArray() ?? Array.Empty<AttributeProperties>())
							.Where(static a => a.QualifiedName != Consts.CustomValidationAttribute)
							.ToArray()
					),
				};
			}

			if (!options.ValidateNestedObjects)
			{
				mapped = mapped with { PropertyIsOfValidatableType = false, CollectionItemsKind = CollectionItemsKind.None };
			}

			mappedProperties.Add(mapped);
		}

		bool copyHooks = options.CopyHooks;
		bool keepTypeParameters = options.TypeName is null;

		return model with
		{
			Name = options.TypeName ?? model.Name,
			Namespace = options.Namespace is null ? model.Namespace
				: options.Namespace.Length == 0 ? null
				: options.Namespace,
			ClassOrRecordKeyword = options.IsRecord is null ? model.ClassOrRecordKeyword
				: options.IsRecord.Value ? "record"
				: "class",
			Accessibility = options.Accessibility ?? model.Accessibility,
			IsSealed = options.TypeName is null ? model.IsSealed : options.IsSealed,
			TypeParameters = keepTypeParameters ? model.TypeParameters : new EquatableArray<string>(Array.Empty<string>()),
			Properties = new EquatableArray<PropertyProperties>(mappedProperties.ToArray()),
			BaseObject = baseObject,
			Methods = copyHooks ? model.Methods : new EquatableArray<MethodProperties>(Array.Empty<MethodProperties>()),
			BeforeValidateMethod = copyHooks ? model.BeforeValidateMethod : null,
			AfterValidateMethod = copyHooks ? model.AfterValidateMethod : null,
		};
	}

	private static ObjectProperties MapObject(
		INamedTypeSymbol type,
		Compilation compilation,
		CancellationToken cancellationToken
	)
	{
		var definition = type.OriginalDefinition;
		var syntaxTree = definition.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree;
		SemanticModel? semanticModel =
			syntaxTree is not null && compilation.ContainsSyntaxTree(syntaxTree)
				? compilation.GetSemanticModel(syntaxTree)
				: null;

		return ValidatableObjectIncrementalValueProvider.MapObject(definition, semanticModel, compilation, cancellationToken);
	}
}
