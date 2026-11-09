// The analyzer runs as a part of the Validly package; it is not a part of the generation API
#if !VALIDLY_SOURCE_GENERATION_API
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Validly.SourceGenerator.Diagnostics;
using Validly.SourceGenerator.Utils;

namespace Validly.SourceGenerator.Analyzers;

/// <summary>
/// Checks the shape of validatable objects, their hooks, custom validation methods and validators,
/// so unsupported signatures are reported clearly instead of producing invalid (or silently ignored) generated code.
/// </summary>
[DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public sealed class ValidatableObjectAnalyzer : DiagnosticAnalyzer
{
	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
		ImmutableArray.Create(
			DiagnosticDescriptors.ValidatableMustBePartial,
			DiagnosticDescriptors.ValidatableMustNotBeNested,
			DiagnosticDescriptors.UnsupportedHookReturnType,
			DiagnosticDescriptors.UnsupportedParameter,
			DiagnosticDescriptors.MissingCustomValidationMethod,
			DiagnosticDescriptors.CustomValidationMethodNotPublic,
			DiagnosticDescriptors.UnsupportedValidationReturnType,
			DiagnosticDescriptors.ValidatorWithoutIsValid
		);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
	}

	private static void AnalyzeType(SymbolAnalysisContext context)
	{
		if (context.Symbol is not INamedTypeSymbol type)
		{
			return;
		}

		if (HasAttribute(type, Consts.ValidatorAttributeQualifiedName))
		{
			AnalyzeValidator(context, type);
		}

		if (HasAttribute(type, Consts.ValidatableAttributeQualifiedName))
		{
			AnalyzeValidatable(context, type);
		}
	}

	private static void AnalyzeValidatable(SymbolAnalysisContext context, INamedTypeSymbol type)
	{
		var location = type.Locations.FirstOrDefault() ?? Location.None;

		if (type.ContainingType is not null)
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ValidatableMustNotBeNested, location, type.Name));
		}

		bool isPartial = type.DeclaringSyntaxReferences.Any(reference =>
			reference.GetSyntax(context.CancellationToken) is TypeDeclarationSyntax declaration
			&& declaration.Modifiers.Any(static m => m.ValueText == "partial")
		);

		if (!isPartial && type.DeclaringSyntaxReferences.Length != 0)
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ValidatableMustBePartial, location, type.Name));
		}

		foreach (var member in type.GetMembers())
		{
			switch (member)
			{
				case IMethodSymbol { Name: Consts.BeforeValidateMethodName or Consts.AfterValidateMethodName } hook
					when hook.MethodKind == MethodKind.Ordinary:
					AnalyzeHook(context, hook);
					break;

				case IPropertySymbol property when HasAttribute(property, Consts.CustomValidationAttribute):
					AnalyzeCustomValidation(context, type, property);
					break;
			}
		}
	}

	private static void AnalyzeHook(SymbolAnalysisContext context, IMethodSymbol hook)
	{
		if (!IsSupportedHookReturnType(hook))
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					DiagnosticDescriptors.UnsupportedHookReturnType,
					hook.Locations.FirstOrDefault(),
					hook.Name,
					hook.ReturnType.ToDisplayString()
				)
			);
		}

		AnalyzeParameters(context, hook, skipFirst: false);
	}

	private static void AnalyzeCustomValidation(SymbolAnalysisContext context, INamedTypeSymbol type, IPropertySymbol property)
	{
		string methodName = Consts.CustomValidationMethodPrefix + property.Name;
		// Declared or inherited
		var method = GetMembersIncludingBase(type, methodName)
			.OfType<IMethodSymbol>()
			.FirstOrDefault(static m => m.MethodKind is MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation);

		// Explicit implementation of the generated interface is named "I...CustomValidation.ValidateX"
		method ??= type.GetMembers()
			.OfType<IMethodSymbol>()
			.FirstOrDefault(m => m.MethodKind == MethodKind.ExplicitInterfaceImplementation && m.Name.EndsWith("." + methodName, StringComparison.Ordinal));

		if (method is null)
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					DiagnosticDescriptors.MissingCustomValidationMethod,
					property.Locations.FirstOrDefault(),
					property.Name,
					methodName
				)
			);
			return;
		}

		if (method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility != Accessibility.Public)
		{
			context.ReportDiagnostic(
				Diagnostic.Create(DiagnosticDescriptors.CustomValidationMethodNotPublic, method.Locations.FirstOrDefault(), method.Name)
			);
		}

		if (!IsSupportedValidationReturnType(method.ReturnType, allowAwaitedValidationResult: false))
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					DiagnosticDescriptors.UnsupportedValidationReturnType,
					method.Locations.FirstOrDefault(),
					method.Name,
					method.ReturnType.ToDisplayString()
				)
			);
		}

		AnalyzeParameters(context, method, skipFirst: false);
	}

	private static void AnalyzeValidator(SymbolAnalysisContext context, INamedTypeSymbol type)
	{
		var isValid = GetMembersIncludingBase(type, Consts.IsValidMethodName).OfType<IMethodSymbol>().FirstOrDefault();

		if (isValid is null || isValid.Parameters.Length == 0)
		{
			context.ReportDiagnostic(
				Diagnostic.Create(DiagnosticDescriptors.ValidatorWithoutIsValid, type.Locations.FirstOrDefault(), type.Name)
			);
			return;
		}

		if (!IsSupportedValidationReturnType(isValid.ReturnType, allowAwaitedValidationResult: false))
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					DiagnosticDescriptors.UnsupportedValidationReturnType,
					isValid.Locations.FirstOrDefault(),
					$"{type.Name}.{isValid.Name}",
					isValid.ReturnType.ToDisplayString()
				)
			);
		}

		// The first parameter is the validated value; the others are resolved
		AnalyzeParameters(context, isValid, skipFirst: true);
	}

	/// <summary>
	/// Parameters are resolved by the generated code: CancellationToken, ValidationContext, ValidationResult or
	/// a service from the IServiceProvider (a reference type)
	/// </summary>
	private static void AnalyzeParameters(SymbolAnalysisContext context, IMethodSymbol method, bool skipFirst)
	{
		for (int index = skipFirst ? 1 : 0; index < method.Parameters.Length; index++)
		{
			var parameter = method.Parameters[index];
			string typeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty).TrimEnd('?');

			bool supported =
				parameter.RefKind == RefKind.None
				&& !parameter.IsParams
				&& (
					typeName is "System.Threading.CancellationToken"
						or Consts.ValidationContextQualifiedName
						or Consts.ValidationResultQualifiedName
						or Consts.ExtendableValidationResultQualifiedName
					|| (parameter.Type.IsReferenceType && parameter.Type.SpecialType != SpecialType.System_String)
				);

			if (!supported)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						DiagnosticDescriptors.UnsupportedParameter,
						parameter.Locations.FirstOrDefault() ?? method.Locations.FirstOrDefault(),
						parameter.Name,
						method.Name
					)
				);
			}
		}
	}

	/// <summary>
	/// void, Task, ValueTask, ValidationResult, ValidationMessage, Validation, IEnumerable&lt;ValidationMessage&gt;,
	/// IAsyncEnumerable&lt;ValidationMessage&gt;, Task/ValueTask of ValidationResult/ValidationMessage/Validation
	/// </summary>
	private static bool IsSupportedHookReturnType(IMethodSymbol hook)
	{
		if (hook.ReturnsVoid)
		{
			return true;
		}

		if (hook.ReturnType is INamedTypeSymbol { IsGenericType: false } named && IsTaskLike(named))
		{
			return true;
		}

		return IsSupportedValidationReturnType(hook.ReturnType, allowAwaitedValidationResult: true);
	}

	/// <summary>
	/// ValidationResult, ValidationMessage, Validation, IEnumerable&lt;ValidationMessage&gt;,
	/// IAsyncEnumerable&lt;ValidationMessage&gt;, Task/ValueTask of ValidationResult/ValidationMessage/Validation
	/// </summary>
	private static bool IsSupportedValidationReturnType(ITypeSymbol returnType, bool allowAwaitedValidationResult)
	{
		if (IsValidlyResultType(returnType))
		{
			return true;
		}

		if (returnType is not INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } generic)
		{
			return false;
		}

		var argument = generic.TypeArguments[0];
		string definition = generic.OriginalDefinition.ToDisplayString();

		return definition switch
		{
			"System.Threading.Tasks.Task<TResult>" or "System.Threading.Tasks.ValueTask<TResult>" =>
				IsValidlyResultType(argument)
				&& (allowAwaitedValidationResult || argument.ToDisplayString().TrimEnd('?') != "Validly.ValidationResult"),
			"System.Collections.Generic.IEnumerable<T>" or "System.Collections.Generic.IAsyncEnumerable<T>" =>
				argument.ToDisplayString().TrimEnd('?') == "Validly.ValidationMessage",
			_ => false,
		};
	}

	private static bool IsValidlyResultType(ITypeSymbol type)
	{
		return type.ToDisplayString().TrimEnd('?')
			is "Validly.ValidationResult"
				or "Validly.ValidationMessage"
				or "Validly.Validation";
	}

	private static bool IsTaskLike(INamedTypeSymbol type)
	{
		return type.ToDisplayString() is "System.Threading.Tasks.Task" or "System.Threading.Tasks.ValueTask";
	}

	private static IEnumerable<ISymbol> GetMembersIncludingBase(INamedTypeSymbol type, string name)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
		{
			foreach (var member in current.GetMembers(name))
			{
				yield return member;
			}
		}
	}

	private static bool HasAttribute(ISymbol symbol, string qualifiedName)
	{
		return symbol.GetAttributes().Any(attr => attr.AttributeClass?.GetQualifiedName() == qualifiedName);
	}
}
#endif
