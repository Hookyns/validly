using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Validly.Naming;
using Validly.SourceGenerator.Utils;

namespace Validly.SourceGenerator.ValueProviders;

/// <summary>
/// Detects the System.Text.Json naming policy of the ASP.NET Core JSON options configured in the compilation,
/// so the JSON names of the properties can be generated at compile time.
/// </summary>
/// <remarks>
/// Only assignments to the HTTP (minimal API) and the MVC JSON options are taken into account, e.g.
/// <c>builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)</c>
/// or <c>.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)</c>.
/// Any other JsonSerializerOptions instance is ignored; it does not have to be the one used by the API.
/// When nothing is found, when the assigned value is not one of the built-in policies (custom policy, null, ...),
/// or when more than one policy is configured, the policy is applied at runtime instead.
/// </remarks>
internal static class JsonNamingPolicyDetectionProvider
{
	private const string PropertyNamingPolicyName = "PropertyNamingPolicy";
	private const string JsonSerializerOptionsName = "System.Text.Json.JsonSerializerOptions";
	private const string JsonNamingPolicyName = "System.Text.Json.JsonNamingPolicy";

	private const string HttpJsonOptionsName = "Microsoft.AspNetCore.Http.Json.JsonOptions";
	private const string HttpJsonOptionsPropertyName = "SerializerOptions";
	private const string MvcJsonOptionsName = "Microsoft.AspNetCore.Mvc.JsonOptions";
	private const string MvcJsonOptionsPropertyName = "JsonSerializerOptions";

	public static IncrementalValueProvider<ValidlyNamingPolicy> Get(
		IncrementalGeneratorInitializationContext initContext
	)
	{
		return initContext
			.SyntaxProvider.CreateSyntaxProvider(IsCandidate, GetPolicy)
			.Where(static policy => policy is not null)
			.Collect()
			.Select(static (policies, _) => Resolve(policies));
	}

	/// <summary>
	/// Cheap syntax check; matches both the property assignments and the object initializers
	/// </summary>
	private static bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken)
	{
		return node is AssignmentExpressionSyntax assignment
			&& GetAssignedMemberName(assignment.Left) == PropertyNamingPolicyName;
	}

	private static string? GetAssignedMemberName(ExpressionSyntax left)
	{
		return left switch
		{
			IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
			_ => null,
		};
	}

	/// <summary>
	/// Returns null when the assignment does not configure the HTTP or MVC JSON options;
	/// <see cref="ValidlyNamingPolicy.None"/> when it does, but the assigned policy is not known at compile time
	/// </summary>
	private static ValidlyNamingPolicy? GetPolicy(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var assignment = (AssignmentExpressionSyntax)context.Node;

		// Object initializers and local variables are plain JsonSerializerOptions instances; not the API ones
		if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
		{
			return null;
		}

		// The assigned property has to be JsonSerializerOptions.PropertyNamingPolicy
		if (
			context.SemanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol
				is not IPropertySymbol { ContainingType: not null } property
			|| property.ContainingType.GetQualifiedName() != JsonSerializerOptionsName
		)
		{
			return null;
		}

		// The options have to be the ones owned by the HTTP or MVC JsonOptions
		if (
			context.SemanticModel.GetSymbolInfo(memberAccess.Expression, cancellationToken).Symbol
				is not IPropertySymbol { ContainingType: not null } optionsProperty
			|| !IsApiJsonOptionsProperty(optionsProperty)
		)
		{
			return null;
		}

		// Anything but a built-in policy (custom policy, null, conditional, ...) can't be resolved at compile time
		if (
			context.SemanticModel.GetSymbolInfo(assignment.Right, cancellationToken).Symbol
				is not IPropertySymbol { ContainingType: not null } policy
			|| policy.ContainingType.GetQualifiedName() != JsonNamingPolicyName
		)
		{
			return ValidlyNamingPolicy.None;
		}

		return policy.Name switch
		{
			"CamelCase" => ValidlyNamingPolicy.CamelCase,
			"SnakeCaseLower" => ValidlyNamingPolicy.SnakeCaseLower,
			"SnakeCaseUpper" => ValidlyNamingPolicy.SnakeCaseUpper,
			"KebabCaseLower" => ValidlyNamingPolicy.KebabCaseLower,
			"KebabCaseUpper" => ValidlyNamingPolicy.KebabCaseUpper,
			_ => ValidlyNamingPolicy.None,
		};
	}

	private static bool IsApiJsonOptionsProperty(IPropertySymbol optionsProperty)
	{
		string containingType = optionsProperty.ContainingType.GetQualifiedName();

		return (containingType == HttpJsonOptionsName && optionsProperty.Name == HttpJsonOptionsPropertyName)
			|| (containingType == MvcJsonOptionsName && optionsProperty.Name == MvcJsonOptionsPropertyName);
	}

	/// <summary>
	/// A single known policy can be used; anything unknown or ambiguous falls back to the runtime
	/// </summary>
	private static ValidlyNamingPolicy Resolve(IEnumerable<ValidlyNamingPolicy?> policies)
	{
		ValidlyNamingPolicy result = ValidlyNamingPolicy.None;

		foreach (ValidlyNamingPolicy? detected in policies)
		{
			ValidlyNamingPolicy policy = detected ?? ValidlyNamingPolicy.None;

			// Unknown policy, or a different one than already found
			if (policy == ValidlyNamingPolicy.None || (result != ValidlyNamingPolicy.None && result != policy))
			{
				return ValidlyNamingPolicy.None;
			}

			result = policy;
		}

		return result;
	}
}
