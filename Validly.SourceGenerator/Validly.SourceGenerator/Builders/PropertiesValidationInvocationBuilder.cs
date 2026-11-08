using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Validly.Naming;
using Validly.SourceGenerator.Dtos;
using Validly.SourceGenerator.Utils;
using Validly.SourceGenerator.Utils.Mapping;
using Validly.SourceGenerator.Utils.SourceTexts;

namespace Validly.SourceGenerator.Builders;

internal class PropertiesValidationInvocationBuilder
{
	private static readonly string InvocationsDelimiter = $"{Environment.NewLine}{Environment.NewLine}";

	private readonly string _ruleClassName;
	private readonly DependenciesTracker _dependenciesTracker;
	private readonly bool _exitEarly;
	private readonly ValidlyNamingPolicy _namingPolicy;

	private readonly List<Func<bool, string>> _invocations = new();

	/// <summary>
	/// List of validation calls for the custom validation methods.
	/// </summary>
	public CallsCollection Calls { get; } = new();

	public PropertiesValidationInvocationBuilder(
		string ruleClassName,
		DependenciesTracker dependenciesTracker,
		bool exitEarly,
		ValidlyNamingPolicy namingPolicy
	)
	{
		_ruleClassName = ruleClassName;
		_dependenciesTracker = dependenciesTracker;
		_exitEarly = exitEarly;
		_namingPolicy = namingPolicy;
	}

	public void AddInvocationForProperty(
		PropertyProperties properties,
		IList<(AttributeProperties, ValidatorProperties)> validators,
		CallsCollection propertyCalls
	)
	{
		string ruleName = $"{properties.PropertyName}Rule";
		int itemNumber = 1;

		foreach ((AttributeProperties _, ValidatorProperties validator) in validators)
		{
			// Add CALL
			var arguments = string.Join(
				", ",
				new[] { properties.PropertyName }.Concat(
					validator.IsValidMethod.Dependencies.Select(dependency =>
					{
						// Track the dependency
						_dependenciesTracker.AddDependency(dependency);

						return $"service{dependency.Name}";
					})
				)
			);

			// > XxxRules.NameRule.Item1.IsValid(XyzProp, serviceSomeService)
			var call = $"{_ruleClassName}.{ruleName}.Item{itemNumber}.IsValid({arguments})";

			propertyCalls.AddValidatorCall(call, validator.IsValidMethod.ReturnTypeType);
			Calls.AddValidatorCall(call, validator.IsValidMethod.ReturnTypeType);

			itemNumber++;
		}

		if (!propertyCalls.Any())
		{
			return;
		}

		// Rendered in Build(), when it is known whether the generated method is async
		int invocationIndex = _invocations.Count;
		_invocations.Add(isAsync => BuildInvocation(properties, propertyCalls, invocationIndex, isAsync));
	}

	private string BuildInvocation(
		PropertyProperties properties,
		CallsCollection propertyCalls,
		int invocationIndex,
		bool isAsync
	)
	{
		var propertyNameLiteral = SymbolDisplay.FormatLiteral(properties.PropertyName, true);
		var propertyNameAndDisplayNameArgs =
			properties.PropertyName == properties.DisplayName
				? propertyNameLiteral
				: $"{propertyNameLiteral}, {SymbolDisplay.FormatLiteral(properties.DisplayName, true)}";

		// The JSON name is resolved at compile time whenever possible; null means the runtime has to do it
		string? jsonName = JsonNameResolver.Resolve(properties, _namingPolicy);

		var initPropertyArgs =
			jsonName is null ? propertyNameAndDisplayNameArgs
			: properties.PropertyName == properties.DisplayName
				? $"{propertyNameLiteral}, null, {SymbolDisplay.FormatLiteral(jsonName, true)}"
			: $"{propertyNameAndDisplayNameArgs}, {SymbolDisplay.FormatLiteral(jsonName, true)}";

		var resultReturn = ReturnStatement.CreateForResult(isAsync);
		var sb = new StringBuilder();

		// ValidationResults
		if (propertyCalls.ValidationResults.Count != 0)
		{
			int resultIndex = 0;

			foreach (ValidatorCallInfo call in propertyCalls.ValidationResults)
			{
				// Unique name; invocations of all the properties share one scope
				string variableName = $"propertyValidationResult{invocationIndex}_{resultIndex++}";

				sb.AppendLine(
					$"""
					var {variableName} = {call.Call};
					if ({variableName} != null) {ReturnStatement.Create(variableName, isAsync)}
					"""
				);
			}
		}

		if (propertyCalls.Voids.Count != 0)
		{
			AppendSyncCalls(sb, propertyCalls.Voids, _exitEarly, resultReturn);
		}

		if (propertyCalls.Messages.Count != 0)
		{
			AppendSyncCalls(sb, propertyCalls.Messages, _exitEarly, resultReturn);
		}

		if (propertyCalls.Validations.Count != 0)
		{
			AppendSyncCalls(sb, propertyCalls.Validations, _exitEarly, resultReturn);
		}

		if (propertyCalls.Enumerables.Count != 0)
		{
			AppendSyncCalls(sb, propertyCalls.Enumerables, _exitEarly, resultReturn);
		}

		if (propertyCalls.Tasks.Count != 0)
		{
			foreach (ValidatorCallInfo call in propertyCalls.Tasks)
			{
				sb.AppendLine($"propertyResult.Add(await {call.Call});");

				if (_exitEarly)
				{
					sb.AppendLine($"if (!propertyResult.IsSuccess) {resultReturn}");
				}
			}
		}

		if (propertyCalls.VoidTasks.Count != 0)
		{
			foreach (ValidatorCallInfo call in propertyCalls.VoidTasks)
			{
				sb.AppendLine($"{call.Call};");
			}
		}

		if (propertyCalls.AsyncEnumerables.Count != 0)
		{
			foreach (ValidatorCallInfo call in propertyCalls.AsyncEnumerables)
			{
				sb.AppendLine($"await propertyResult.AddAsync({call.Call});");

				if (_exitEarly)
				{
					sb.AppendLine($"if (!propertyResult.IsSuccess) {resultReturn}");
				}
			}
		}

		return $$"""
			// Validate "{{properties.PropertyName}}" property
			context.SetProperty({{propertyNameAndDisplayNameArgs}});
			propertyResult = result.InitProperty({{initPropertyArgs}});
			{{sb}}
			""";
	}

	private static void AppendSyncCalls(
		StringBuilder sb,
		List<ValidatorCallInfo> calls,
		bool exitEarly,
		string resultReturn
	)
	{
		foreach (ValidatorCallInfo call in calls)
		{
			sb.AppendLine($"propertyResult.Add({call.Call});");

			if (exitEarly)
			{
				sb.AppendLine($"if (!propertyResult.IsSuccess) {resultReturn}");
			}
		}
	}

	/// <summary>
	/// Builds the invocations of all the properties
	/// </summary>
	/// <param name="isAsync">Whether the generated ValidateAsync method is async</param>
	/// <returns></returns>
	public string Build(bool isAsync)
	{
		if (_invocations.Count == 0)
		{
			return string.Empty;
		}

		return $"""

			{Consts.ExpandablePropertyValidationResultGlobalRef} propertyResult;

			{string.Join(InvocationsDelimiter, _invocations.Select(invocation => invocation(isAsync)))}

			""";
	}
}
