using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Numbers;
using Validly.Extensions.Validators.Strings;
using Validly.Validators;

namespace Validly.Tests;

[Validatable]
public partial class GenericObject<T>
{
	[Between(1, 10)]
	public int Number { get; set; }

	public T? Value { get; set; }

	public bool AfterValidateCalled { get; private set; }

	private ValidationMessage? AfterValidate()
	{
		AfterValidateCalled = true;
		return null;
	}
}

/// <summary>
/// Non-generic object with the same name as the generic one; they must not collide
/// </summary>
[Validatable]
public partial class GenericObject
{
	[Required]
	public string? Name { get; set; }
}

[Validatable]
public partial class GenericObjectWithConstraints<TKey, TItem>
	where TKey : struct
	where TItem : class, new()
{
	public TKey Key { get; set; }

	[Required]
	public TItem? Item { get; set; }

	[CustomValidation]
	[MinLength(2)]
	public string? Name { get; set; }

	IEnumerable<ValidationMessage> IGenericObjectWithConstraintsCustomValidation<TKey, TItem>.ValidateName()
	{
		if (Name == "invalid")
		{
			yield return new ValidationMessage("Name is invalid", "GenericObject.Name");
		}
	}
}

[Validatable]
public partial record GenericRecord<T>
{
	[Required]
	public T? Value { get; set; }
}

[Validatable]
public partial class GenericObjectParent
{
	public GenericObject<int> Nested { get; set; } = new();
}

public class GenericValidatableObjectTests
{
	[Fact]
	public void GenericObject_Valid()
	{
		var request = new GenericObject<string> { Number = 5 };

		using var result = request.Validate();

		Assert.True(result.IsSuccess);
		Assert.True(request.AfterValidateCalled);
	}

	[Fact]
	public void GenericObject_Invalid()
	{
		using var result = new GenericObject<string> { Number = 50 }.Validate();

		Assert.False(result.IsSuccess);
		Assert.Equal(nameof(GenericObject<string>.Number), result.Properties.Single(x => !x.IsSuccess).PropertyPath);
	}

	[Fact]
	public void GenericObject_ImplementsValidatable()
	{
		Assert.IsAssignableFrom<IValidatable>(new GenericObject<int>());
		Assert.IsAssignableFrom<IValidatable>(new GenericRecord<int>());
	}

	[Fact]
	public void NonGenericObjectWithSameName_Valid()
	{
		using var result = new GenericObject { Name = "name" }.Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void NonGenericObjectWithSameName_Invalid()
	{
		using var result = new GenericObject().Validate();

		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void GenericObjectWithConstraints_Valid()
	{
		using var result = new GenericObjectWithConstraints<int, object>
		{
			Item = new object(),
			Name = "name",
		}.Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void GenericObjectWithConstraints_RequiredTypeParameterProperty_Invalid()
	{
		using var result = new GenericObjectWithConstraints<int, object> { Name = "name" }.Validate();

		Assert.Equal(
			nameof(GenericObjectWithConstraints<int, object>.Item),
			result.Properties.Single(x => !x.IsSuccess).PropertyPath
		);
	}

	[Fact]
	public void GenericObjectWithConstraints_CustomValidation_Invalid()
	{
		using var result = new GenericObjectWithConstraints<int, object>
		{
			Item = new object(),
			Name = "invalid",
		}.Validate();

		Assert.Equal(
			nameof(GenericObjectWithConstraints<int, object>.Name),
			result.Properties.Single(x => !x.IsSuccess).PropertyPath
		);
	}

	[Fact]
	public void GenericRecord_Valid()
	{
		using var result = new GenericRecord<string> { Value = "value" }.Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void GenericRecord_Invalid()
	{
		using var result = new GenericRecord<string>().Validate();

		Assert.False(result.IsSuccess);
	}

	[Fact]
	public async Task NestedGenericObject_Valid()
	{
		using var result = await new GenericObjectParent { Nested = { Number = 5 } }.ValidateAsync();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task NestedGenericObject_Invalid()
	{
		using var result = await new GenericObjectParent { Nested = { Number = 50 } }.ValidateAsync();

		Assert.Equal(
			$"{nameof(GenericObjectParent.Nested)}/{nameof(GenericObject<int>.Number)}",
			result.Properties.Single(x => !x.IsSuccess).PropertyPath
		);
	}
}
