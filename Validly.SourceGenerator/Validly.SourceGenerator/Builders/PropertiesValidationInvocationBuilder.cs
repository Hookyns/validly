using System.Text;
using Validly.SourceGenerator.Dtos;
using Validly.SourceGenerator.Utils.Mapping;

namespace Validly.SourceGenerator.Builders;

internal class PropertiesValidationInvocationBuilder
{
	private static readonly string InvocationsDelimiter = $"{Environment.NewLine}{Environment.NewLine}";

	private readonly string _ruleClassName;
	private readonly DependenciesTracker _dependenciesTracker;
	private readonly bool _exitEarly;

	private readonly List<string> _invocations = new();

	/// <summary>
	/// List of validation calls for the custom validation methods.
	/// </summary>
	public CallsCollection Calls { get; } = new();

	public PropertiesValidationInvocationBuilder(
		string ruleClassName,
		DependenciesTracker dependenciesTracker,
		bool exitEarly
	)
	{
		_ruleClassName = ruleClassName;
		_dependenciesTracker = dependenciesTracker;
		_exitEarly = exitEarly;
	}

	public void AddInvocationForProperty(
		PropertyProperties properties,
		IList<(AttributeProperties, ValidatorProperties)> validators,
		CallsCollection propertyCalls,
		bool isAsync
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

		var propertyNameAndDisplayNameArgs =
			properties.PropertyName == properties.DisplayName
				? $"\"{properties.PropertyName}\""
				: $"\"{properties.PropertyName}\", \"{properties.DisplayName}\"";

		var sb = new StringBuilder();

		// ValidationResults
		if (propertyCalls.ValidationResults.Count != 0)
		{
			foreach (ValidatorCallInfo call in propertyCalls.ValidationResults)
			{
				sb.AppendLine(
					$"""
					var propertyValidationResult = {call.Call};
					if (propertyValidationResult != null) return propertyValidationResult;
					"""
				);
			}
		}

		var anyAsync = propertyCalls.AnyAsync() || isAsync;

		var resultReturn = anyAsync
			? $"new ValueTask<{Consts.ValidationResultGlobalRef}>(({Consts.ValidationResultGlobalRef})result)"
			: $"({Consts.ValidationResultGlobalRef})result";

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
					sb.AppendLine($"if (!propertyResult.IsSuccess) return ({Consts.ValidationResultGlobalRef})result;");
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
					sb.AppendLine($"if (!propertyResult.IsSuccess) return ({Consts.ValidationResultGlobalRef})result;");
				}
			}
		}

		_invocations.Add(
			$$"""
			// Validate "{{properties.PropertyName}}" property
			context.SetProperty({{propertyNameAndDisplayNameArgs}});
			propertyResult = result.InitProperty({{propertyNameAndDisplayNameArgs}});
			{{sb}}
			"""
		);
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
				sb.AppendLine($"if (!propertyResult.IsSuccess) return {resultReturn};");
			}
		}
	}

	public string Build()
	{
		if (_invocations.Count == 0)
		{
			return string.Empty;
		}

		return $"""

			{Consts.ExpandablePropertyValidationResultGlobalRef} propertyResult;

			{string.Join(InvocationsDelimiter, _invocations)}

			""";
	}
}
