using Validly.Validators;

namespace Validly.Tests.ServiceProvider.Fixtures;

[Validatable]
internal partial record ValidatableObject([property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(IDependency dependency)
	{
		var number = dependency.GetNumber(Property);

		if (number % 2 != 0)
		{
			yield break;
		}

		yield return new ValidationMessage($"Number {number} based on property {Property} is even", "Property.Even");
	}
}
