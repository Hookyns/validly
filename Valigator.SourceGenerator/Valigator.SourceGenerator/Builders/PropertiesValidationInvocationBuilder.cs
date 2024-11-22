using Valigator.SourceGenerator.Dtos;
using Valigator.SourceGenerator.Utils.Mapping;

namespace Valigator.SourceGenerator.Builders;

internal class PropertiesValidationInvocationBuilder
{
	private static readonly string InvocationsDelimiter = $"{Environment.NewLine}{Environment.NewLine}";

	private readonly string _ruleClassName;
	private readonly ValigatorConfiguration _config;
	private readonly DependenciesTracker _dependenciesTracker;

	private readonly List<string> _invocations = new();

	/// <summary>
	/// List of validation calls for the custom validation methods.
	/// </summary>
	public CallsCollection Calls { get; } = new();

	public PropertiesValidationInvocationBuilder(
		string ruleClassName,
		ValigatorConfiguration config,
		DependenciesTracker dependenciesTracker
	)
	{
		_ruleClassName = ruleClassName;
		_config = config;
		_dependenciesTracker = dependenciesTracker;
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
					validator.IsValidMethod.Dependencies.Select(service =>
					{
						// Track the dependency
						_dependenciesTracker.AddDependency(service);

						return $"service{service}";
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

		var earlyExitCheck = _config.ExitEarly
			? """

				if (!propertyResult.IsSuccess)
				{
					result.AddPropertyResult(propertyResult);
					return result;
				}
				"""
			: string.Empty;

		_invocations.Add(
			$$"""
			// Validate {{properties.PropertyName}} property
			context.SetProperty("{{properties.PropertyName}}");
			propertyResult = {{Consts.PropertyValidationResultGlobalRef}}.Create("{{properties.PropertyName}}");
			{{string.Join(
				Environment.NewLine,
				propertyCalls.ValidationResults.Select(static call => $$"""
					  var propertyValidationResult = propertyResult.AddValidationMessage({{call.Call}});
					  if (propertyValidationResult != null) return propertyValidationResult;
					  """)
			)}}
			{{string.Join(
				Environment.NewLine,
				propertyCalls.Messages.Select(call => $"propertyResult.AddValidationMessage({call.Call});")
			)}}{{earlyExitCheck}}{{(propertyCalls.Messages.Count == 0 ? string.Empty : Environment.NewLine)}}{{string.Join(
				Environment.NewLine,
				propertyCalls.Validations.Select(call => $"propertyResult.AddValidationMessage({call.Call});")
			)}}{{earlyExitCheck}}{{string.Join(
				Environment.NewLine,
				propertyCalls.Enumerables.Select(call => $"propertyResult.AddValidationMessages({call.Call});")
			)}}{{earlyExitCheck}}{{string.Join(
				Environment.NewLine,
				propertyCalls.Tasks.Select(call => $"propertyResult.AddValidationMessages(await {call.Call});")
			)}}{{earlyExitCheck}}{{string.Join(
				Environment.NewLine,
				propertyCalls.AsyncEnumerables.Select(call => $"await propertyResult.AddValidationMessages({call.Call});")
			)}}{{earlyExitCheck}}
			result.AddPropertyResult(propertyResult);
			"""
		);
	}

	public string Build()
	{
		if (_invocations.Count == 0)
		{
			return string.Empty;
		}

		return $"""

			{Consts.PropertyValidationResultGlobalRef} propertyResult;

			{string.Join(InvocationsDelimiter, _invocations)}

			""";
	}
}
