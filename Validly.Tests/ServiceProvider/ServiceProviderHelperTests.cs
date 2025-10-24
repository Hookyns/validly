using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Validly.Tests.ServiceProvider.Fixtures;

namespace Validly.Tests.ServiceProvider;

public class ServiceProviderHelperTests
{
	[Theory]
	[InlineData("Odd")]
	[InlineData("Even")]
	public void Validate_DependencyReturnedValue_ResultChanged(string property)
	{
		var dependency = Substitute.For<IDependency>();
		dependency.GetNumber(Arg.Any<string>()).Returns(property is "Odd" ? 1 : 2);
		var serviceProvider = new ServiceCollection().AddSingleton(dependency).BuildServiceProvider();

		var validatable = new ValidatableObject("Odd");
		var result = validatable.Validate(serviceProvider);

		Assert.Equal(result.IsSuccess, property is "Odd");
	}

	[Theory]
	[InlineData(StaticKeys.StringKey)]
	[InlineData(StaticKeys.IntKey)]
	[InlineData(StaticKeys.EnumKey)]
	[InlineData(StaticKeys.BoolKey)]
	[InlineData(StaticKeys.CharKey)]
	[InlineData(StaticKeys.UnknownStringKey, false)]
	public async Task Validate_DependencyReturnedValue_KeyReturnedValue(object key, bool shouldSucceed = true)
	{
		var dependency = Substitute.For<IDependency>();
		dependency.GetNumber(Arg.Any<string>()).Returns(1);
		var serviceProvider = new ServiceCollection().AddKeyedSingleton(key, dependency).BuildServiceProvider();

		var validatable = StaticKeys.GetValidatable(key);

		if (shouldSucceed)
		{
			var result = await validatable!.ValidateAsync(serviceProvider);
			Assert.Equal(shouldSucceed, result.IsSuccess);
		}
		else
		{
			await Assert.ThrowsAsync<InvalidOperationException>(async () =>
				await validatable!.ValidateAsync(serviceProvider)
			);
		}
	}
}
