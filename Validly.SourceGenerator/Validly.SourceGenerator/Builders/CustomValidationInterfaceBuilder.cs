using Validly.SourceGenerator.Utils;
using Validly.SourceGenerator.Utils.Mapping;

namespace Validly.SourceGenerator.Builders;

internal class CustomValidationInterfaceBuilder
{
	private static readonly string MethodDelimiter = $"{Environment.NewLine}{Environment.NewLine}\t";

	private readonly string _interfaceName;
	private readonly DependenciesTracker _dependenciesTracker;
	private Dictionary<string, MethodProperties> _validatableObjectMethods = null!;
	private readonly List<string> _customValidationMethods = new();

	/// <summary>
	/// List of validation calls for the custom validation methods.
	/// </summary>
	public CallsCollection Calls { get; } = new();

	public CustomValidationInterfaceBuilder(string interfaceName, DependenciesTracker dependenciesTracker)
	{
		_interfaceName = interfaceName;
		_dependenciesTracker = dependenciesTracker;
	}

	public bool HasAnyMethod() => _customValidationMethods.Count > 0;

	public CustomValidationInterfaceBuilder WithMethods(Dictionary<string, MethodProperties> methods)
	{
		_validatableObjectMethods = methods;
		return this;
	}

	public void AddCustomValidationForProperty(PropertyProperties properties, CallsCollection propertyCalls)
	{
		// Generate CUSTOM validation method to the interface

		if (_validatableObjectMethods.TryGetValue($"Validate{properties.PropertyName}", out var existingMethod))
		{
			// Full return type name including generic arguments
			var nullability = (existingMethod.ReturnTypeType & ReturnTypeType.Nullable) != 0 ? "?" : string.Empty;
			var returnType =
				existingMethod.ReturnType
				+ (
					existingMethod.ReturnTypeGenericArgument is null
						? nullability
						: $"<{existingMethod.ReturnTypeGenericArgument}{nullability}>"
				);

			// Dependencies
			var dependencies = new List<string>();

			foreach (var dependency in existingMethod.Dependencies)
			{
				// Track the dependency
				_dependenciesTracker.AddDependency(dependency);

				// Create method parameter
				dependencies.Add($"{dependency.Name} {dependency.Name.Substring(0, 1).ToLower() + dependency.Name.Substring(1)}");
			}

			_customValidationMethods.Add(
				$"""

				/// <summary>
				/// Custom validation method for property '{properties.PropertyName}'
				/// </summary>
				{returnType} Validate{properties.PropertyName}({string.Join(", ", dependencies)});
				""".Indent()
			);

			// Add CALL
			var arguments = string.Join(
				", ",
				existingMethod.Dependencies.Select(dependency =>
					dependency.Name is Consts.CancellationTokenName ? "ct" : $"service{dependency.Name}"
				)
			);
			var call = $"customValidator.Validate{properties.PropertyName}({arguments})";

			Calls.AddValidatorCall(call, existingMethod.ReturnTypeType);
			propertyCalls.AddValidatorCall(call, existingMethod.ReturnTypeType);
		}
		else
		{
			_customValidationMethods.Add(
				$"""

				/// <summary>
				/// Custom validation method for property '{properties.PropertyName}'
				/// </summary>
				IEnumerable<{Consts.ValidationMessageGlobalRef}> Validate{properties.PropertyName}();
				""".Indent()
			);

			// Add CALL
			var call = $"customValidator.Validate{properties.PropertyName}()";
			Calls.AddValidatorCall(call, ReturnTypeType.Enumerable);
			propertyCalls.AddValidatorCall(call, ReturnTypeType.Enumerable);
		}
	}

	public string Build()
	{
		if (_customValidationMethods.Count == 0)
		{
			return string.Empty;
		}

		return $$"""
			internal interface {{_interfaceName}}
			{
				{{string.Join(MethodDelimiter, _customValidationMethods)}}
			}
			""";
	}
}
