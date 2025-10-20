using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Validly.Tests.ServiceProvider.Fixtures;

namespace Validly.Tests.ServiceProvider;

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

	[Theory]
	[InlineData(StaticKeys.StringKey)]
	[InlineData(StaticKeys.IntKey)]
	[InlineData(StaticKeys.EnumKey)]
	[InlineData(StaticKeys.UnknowStringKey, false)]
	public async Task Validate_DependencyReturnedValue_KeyReturnedValue(object key, bool shouldSucceed = true)
	{
		var dependency = Substitute.For<IDependency>();
		dependency.GetNumber(Arg.Any<string>()).Returns(1);
		var serviceCollection = new ServiceCollection()
			.AddKeyedSingleton(key, dependency)
			.BuildServiceProvider();

		var validatable = StaticKeys.GetValidatable(key);

		if (shouldSucceed)
		{
			var result =
				await validatable!.ValidateAsync(serviceCollection);
			Assert.Equal(shouldSucceed, result.IsSuccess);
		}
		else
		{
			await Assert.ThrowsAsync<InvalidOperationException>(async () =>
				await validatable!.ValidateAsync(serviceCollection));
		}
	}
}