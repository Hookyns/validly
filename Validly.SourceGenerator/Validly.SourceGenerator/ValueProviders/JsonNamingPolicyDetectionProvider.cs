using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Validly.Naming;
using Validly.SourceGenerator.Utils;

namespace Validly.SourceGenerator.ValueProviders;

/// <summary>
/// Detects the System.Text.Json naming policy configured in the compilation, so the JSON names of the properties
/// can be generated at compile time.
/// </summary>
/// <remarks>
/// Only assignments of the built-in policies to JsonSerializerOptions.PropertyNamingPolicy are recognized, e.g.
/// <c>builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)</c>.
/// When nothing is found, or when the compilation configures more than one policy, the policy is applied at runtime instead.
/// </remarks>
internal static class JsonNamingPolicyDetectionProvider
{
	private const string PropertyNamingPolicyName = "PropertyNamingPolicy";
	private const string JsonSerializerOptionsName = "System.Text.Json.JsonSerializerOptions";
	private const string JsonNamingPolicyName = "System.Text.Json.JsonNamingPolicy";

	public static IncrementalValueProvider<ValidlyNamingPolicy> Get(
		IncrementalGeneratorInitializationContext initContext
	)
	{
		return initContext
			.SyntaxProvider.CreateSyntaxProvider(IsCandidate, GetPolicy)
			.Where(static policy => policy != ValidlyNamingPolicy.None)
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

	private static ValidlyNamingPolicy GetPolicy(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var assignment = (AssignmentExpressionSyntax)context.Node;

		// The assigned property has to be JsonSerializerOptions.PropertyNamingPolicy
		if (
			context.SemanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol
				is not IPropertySymbol { ContainingType: not null } property
			|| property.ContainingType.GetQualifiedName() != JsonSerializerOptionsName
		)
		{
			return ValidlyNamingPolicy.None;
		}

		// The assigned value has to be one of the built-in policies
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

	/// <summary>
	/// A single policy can be used; anything ambiguous falls back to the runtime
	/// </summary>
	private static ValidlyNamingPolicy Resolve(IEnumerable<ValidlyNamingPolicy> policies)
	{
		ValidlyNamingPolicy result = ValidlyNamingPolicy.None;

		foreach (ValidlyNamingPolicy policy in policies)
		{
			if (result == ValidlyNamingPolicy.None)
			{
				result = policy;
				continue;
			}

			if (result != policy)
			{
				return ValidlyNamingPolicy.None;
			}
		}

		return result;
	}
}
