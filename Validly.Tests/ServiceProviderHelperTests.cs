using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Validly.Validators;

namespace Validly.Tests;

public class ServiceProviderHelperTests
{
	[Theory]
	[InlineData("Property")]
	[InlineData("Not Property")]
	public void Validate_DependencyReturnedValue_ResultChanged(string property)
	{
		var dependency = Substitute.For<IDependency>();
		dependency.GetNumber(Arg.Any<string>()).Returns(property is "Property" ? 1 : 2);
		var validatable = new ValidatableObject("Property");
		var result = validatable.Validate(new ServiceCollection().AddSingleton(dependency).BuildServiceProvider());
		Assert.Equal(result.IsSuccess, property is "Property");
	}
}

[Validatable]
internal partial record ValidatableObject(
	[property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(
		IDependency dependency)
	{
		var number = dependency.GetNumber(Property);
		if (number % 2 != 0)
			yield break;
		yield return new ValidationMessage(
			$"Number {number} based on property {Property} is even",
			"Property.Even");
	}
}

public interface IDependency
{
	int GetNumber(string property);
}