using Microsoft.Extensions.DependencyInjection;
using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Numbers;
using Validly.Extensions.Validators.Strings;
using Validly.Validators;

namespace Validly.Tests;

// GitHub #29 - the minimal reproduction
[Validatable]
public partial class InheritanceEmptyBase { }

[Validatable]
public partial class InheritanceEmptyDerived : InheritanceEmptyBase { }

[Validatable]
public partial class InheritanceAnimal
{
	[Required]
	[MinLength(2)]
	public string? Name { get; set; }

	public List<string> HookCalls { get; } = new();

	private IEnumerable<ValidationMessage> AfterValidate(ValidationContext context)
	{
		HookCalls.Add($"Animal:{context.RootObject.GetType().Name}");
		yield break;
	}
}

[Validatable]
public partial class InheritanceDog : InheritanceAnimal
{
	[Between(0, 30)]
	public int Age { get; set; }

	private IEnumerable<ValidationMessage> AfterValidate()
	{
		HookCalls.Add("Dog");

		if (Name == "Rex")
		{
			yield return new ValidationMessage("Rex is taken", "Dog.Rex");
		}
	}
}

[Validatable]
public sealed partial class InheritanceSealedDog : InheritanceAnimal
{
	[Between(0, 30)]
	public int Age { get; set; }
}

// Non-validatable class in between
public class InheritancePlainMiddle : InheritanceAnimal
{
	public string? NotValidated { get; set; }
}

[Validatable]
public partial class InheritanceThroughPlainMiddle : InheritancePlainMiddle
{
	[Required]
	public string? Owner { get; set; }
}

// Three validatable levels
[Validatable]
public partial class InheritancePuppy : InheritanceDog
{
	[MaxLength(3)]
	public string? Toy { get; set; }
}

// Records
[Validatable]
public partial record InheritanceBaseRecord
{
	[Required]
	public string? Id { get; init; }
}

[Validatable]
public partial record InheritanceDerivedRecord : InheritanceBaseRecord
{
	[MinLength(3)]
	public string? Title { get; init; }
}

// Generic base
[Validatable]
public partial class InheritanceGenericBase<TValue>
{
	[Required]
	public TValue? Value { get; set; }
}

[Validatable]
public partial class InheritanceGenericDerived : InheritanceGenericBase<string>
{
	[MinLength(3)]
	public string? Label { get; set; }
}

// Dependency injection; derived requires the service provider, the base does not
public interface IInheritanceNameRegistry
{
	bool IsKnown(string? name);
}

[Validatable]
public partial class InheritanceDerivedWithService : InheritanceAnimal
{
	[CustomValidation]
	public string? Breed { get; set; }

	public Validation ValidateBreed(IInheritanceNameRegistry registry)
	{
		return registry.IsKnown(Breed) ? Validation.Success() : Validation.Error("Unknown breed", "Breed.Unknown");
	}
}

// Base requires the service provider, derived does not
[Validatable]
public partial class InheritanceBaseWithService
{
	[CustomValidation]
	public string? Code { get; set; }

	public Validation ValidateCode(IInheritanceNameRegistry registry)
	{
		return registry.IsKnown(Code) ? Validation.Success() : Validation.Error("Unknown code", "Code.Unknown");
	}
}

[Validatable]
public partial class InheritanceDerivedFromServiceBase : InheritanceBaseWithService
{
	[Required]
	public string? Description { get; set; }
}

// Async base (async hook), sync derived
[Validatable]
public partial class InheritanceAsyncBase
{
	[Required]
	public string? Name { get; set; }

	private async Task<ValidationMessage?> AfterValidate()
	{
		await Task.Yield();
		return Name == "async" ? new ValidationMessage("Async error", "Async.Error") : null;
	}
}

[Validatable]
public partial class InheritanceDerivedFromAsyncBase : InheritanceAsyncBase
{
	[MinLength(3)]
	public string? Other { get; set; }
}

// Polymorphic nested object
[Validatable]
public partial class InheritanceOwner
{
	public InheritanceAnimal? Pet { get; set; }
}

public class InheritedValidatableObjectTests
{
	private sealed class NameRegistry(params string[] known) : IInheritanceNameRegistry
	{
		public bool IsKnown(string? name) => name is not null && known.Contains(name);
	}

	private static IServiceProvider CreateServiceProvider(params string[] known) =>
		new ServiceCollection()
			.AddSingleton<IInheritanceNameRegistry>(new NameRegistry(known))
			.BuildServiceProvider();

	[Fact]
	public void EmptyBaseAndDerived_Valid()
	{
		using var result = new InheritanceEmptyDerived().Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void Derived_ContainsBaseResultsFirst()
	{
		using var result = new InheritanceDog { Name = "R", Age = 50 }.Validate();

		Assert.False(result.IsSuccess);
		Assert.Equal(["Name", "Age"], result.Properties.Select(p => p.PropertyPath));
		Assert.Single(result.Properties[0].Messages);
		Assert.Single(result.Properties[1].Messages);
	}

	[Fact]
	public void Derived_Valid()
	{
		using var result = new InheritanceDog { Name = "Bobby", Age = 5 }.Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task Derived_HooksOfBothLevelsAreCalled_InSharedContext()
	{
		var dog = new InheritanceDog { Name = "Rex", Age = 5 };

		using var result = await dog.ValidateAsync();

		// Base hook sees the derived object as the root; the context is shared
		Assert.Equal(["Animal:InheritanceDog", "Dog"], dog.HookCalls);
		Assert.Equal("Dog.Rex", Assert.Single(result.Global).ResourceKey);
	}

	[Fact]
	public void Derived_ValidatedThroughBaseReference_ValidatesDerived()
	{
		InheritanceAnimal animal = new InheritanceDog { Name = "Bobby", Age = 50 };

		using var result = animal.Validate();

		Assert.Equal(["Name", "Age"], result.Properties.Select(p => p.PropertyPath));
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void SealedDerived_ContainsBaseResults()
	{
		using var result = new InheritanceSealedDog { Name = null, Age = 50 }.Validate();

		Assert.Equal(["Name", "Age"], result.Properties.Select(p => p.PropertyPath));
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void NonValidatableClassInBetween_NearestValidatableAncestorIsValidated()
	{
		using var result = new InheritanceThroughPlainMiddle { Name = null, Owner = null }.Validate();

		Assert.Equal(["Name", "Owner"], result.Properties.Select(p => p.PropertyPath));
		Assert.All(result.Properties, p => Assert.False(p.IsSuccess));
	}

	[Fact]
	public void ThreeLevels_AllLevelsAreValidated()
	{
		var puppy = new InheritancePuppy
		{
			Name = "P",
			Age = 50,
			Toy = "Bone",
		};

		using var result = puppy.Validate();

		Assert.Equal(["Name", "Age", "Toy"], result.Properties.Select(p => p.PropertyPath));
		Assert.All(result.Properties, p => Assert.False(p.IsSuccess));
		Assert.Equal(["Animal:InheritancePuppy", "Dog"], puppy.HookCalls);
	}

	[Fact]
	public void DerivedRecord_ContainsBaseResults()
	{
		using var result = new InheritanceDerivedRecord { Id = null, Title = "ab" }.Validate();

		Assert.Equal(["Id", "Title"], result.Properties.Select(p => p.PropertyPath));
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void DerivedRecord_EqualityIsNotAffectedByValidation()
	{
		var record = new InheritanceDerivedRecord { Id = "1", Title = "abc" };
		var copy = record with { };

		record.Validate().Dispose();

		Assert.Equal(record, copy);
	}

	[Fact]
	public void DerivedFromGenericBase_ContainsBaseResults()
	{
		using var result = new InheritanceGenericDerived { Value = null, Label = "ab" }.Validate();

		Assert.Equal(["Value", "Label"], result.Properties.Select(p => p.PropertyPath));
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public async Task DerivedRequiresServiceProvider_InheritedMethodsWithoutServiceProviderThrow()
	{
		var obj = new InheritanceDerivedWithService { Name = "Bobby", Breed = "Husky" };

		await Assert.ThrowsAsync<InvalidOperationException>(async () => await obj.ValidateAsync());
		Assert.Throws<InvalidOperationException>(() => obj.Validate());
	}

	[Fact]
	public async Task DerivedRequiresServiceProvider_ValidatedWithServiceProvider()
	{
		var serviceProvider = CreateServiceProvider("Husky");

		using (var valid = await new InheritanceDerivedWithService { Name = "Bobby", Breed = "Husky" }.ValidateAsync(serviceProvider))
		{
			Assert.True(valid.IsSuccess);
		}

		using var invalid = await new InheritanceDerivedWithService { Name = "B", Breed = "Pug" }.ValidateAsync(serviceProvider);
		Assert.Equal(["Name", "Breed"], invalid.Properties.Select(p => p.PropertyPath));
		Assert.All(invalid.Properties, p => Assert.False(p.IsSuccess));

		// Through the interface, like the ASP.NET filter does
		using var throughInterface = await ((IValidatable)new InheritanceDerivedWithService { Name = "B", Breed = "Husky" }).ValidateAsync(serviceProvider, default);
		Assert.Equal(["Name", "Breed"], throughInterface.Properties.Select(p => p.PropertyPath));
		Assert.False(throughInterface.IsSuccess);
	}

	[Fact]
	public void BaseRequiresServiceProvider_DerivedPassesIt()
	{
		var serviceProvider = CreateServiceProvider("A1");

		using var valid = new InheritanceDerivedFromServiceBase { Code = "A1", Description = "d" }.Validate(serviceProvider);
		Assert.True(valid.IsSuccess);

		using var invalid = new InheritanceDerivedFromServiceBase { Code = "XX", Description = null }.Validate(serviceProvider);
		Assert.Equal(["Code", "Description"], invalid.Properties.Select(p => p.PropertyPath));
		Assert.All(invalid.Properties, p => Assert.False(p.IsSuccess));
	}

	[Fact]
	public async Task AsyncBase_DerivedIsAsync()
	{
		using var result = await new InheritanceDerivedFromAsyncBase { Name = "async", Other = "ab" }.ValidateAsync();

		Assert.Equal(["Name", "Other"], result.Properties.Select(p => p.PropertyPath));
		Assert.Equal("Async.Error", Assert.Single(result.Global).ResourceKey);
		Assert.False(result.Properties[1].IsSuccess);
	}

	[Fact]
	public async Task NestedPolymorphicObject_DerivedIsValidated()
	{
		var owner = new InheritanceOwner { Pet = new InheritanceDog { Name = "Bobby", Age = 50 } };

		using var result = await owner.ValidateAsync();

		Assert.Equal(["Pet/Name", "Pet/Age"], result.Properties.Select(p => p.PropertyPath));
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public async Task RepeatedValidations_DoNotShareContextOrResults()
	{
		var dog = new InheritanceDog { Name = "R", Age = 5 };

		for (int i = 0; i < 50; i++)
		{
			using var result = await dog.ValidateAsync();
			Assert.Equal(["Name", "Age"], result.Properties.Select(p => p.PropertyPath));
			Assert.False(result.Properties[0].IsSuccess);
			Assert.True(result.Properties[1].IsSuccess);
		}
	}
}

// Hook overriding a virtual hook of a non-validatable base is called by the validatable class itself
public abstract class InheritancePlainBaseWithHook
{
	protected virtual IEnumerable<ValidationMessage> AfterValidate()
	{
		yield break;
	}
}

[Validatable]
public partial class InheritanceOverridesPlainHook : InheritancePlainBaseWithHook
{
	protected override IEnumerable<ValidationMessage> AfterValidate()
	{
		yield return new ValidationMessage("Plain hook override", "PlainHook");
	}
}

// Virtual hook of a validatable base overridden by a validatable derived class; called once
[Validatable]
public partial class InheritanceVirtualHookBase
{
	public int Calls { get; set; }

	protected virtual IEnumerable<ValidationMessage> AfterValidate()
	{
		Calls++;
		yield break;
	}
}

[Validatable]
public partial class InheritanceVirtualHookDerived : InheritanceVirtualHookBase
{
	protected override IEnumerable<ValidationMessage> AfterValidate()
	{
		Calls += 10;
		yield break;
	}
}

public class InheritedHookTests
{
	[Fact]
	public void HookOverridingPlainBaseHook_IsCalled()
	{
		using var result = new InheritanceOverridesPlainHook().Validate();

		Assert.Equal("PlainHook", Assert.Single(result.Global).ResourceKey);
	}

	[Fact]
	public void HookOverridingValidatableBaseHook_IsCalledOnce()
	{
		var obj = new InheritanceVirtualHookDerived();

		obj.Validate().Dispose();

		Assert.Equal(10, obj.Calls);
	}

	[Fact]
	public void ObjectWithNestedValidatable_HasSynchronousValidate()
	{
		// Nested objects make the validation async, but Validate() is available as long as it completes synchronously
		using var result = new InheritanceOwner { Pet = new InheritanceDog { Name = "B", Age = 1 } }.Validate();

		Assert.Equal("Pet/Name", Assert.Single(result.Properties, p => !p.IsSuccess).PropertyPath);
	}
}
