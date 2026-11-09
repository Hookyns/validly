using Validly.Validators;

namespace Validly.Tests;

// GitHub #23 - validation without Validly interfaces on the validated object
[Validatable(Detached = true)]
public partial class DetachedOrder
{
	[Required]
	[MinLength(3)]
	public string? Number { get; set; }

	public DetachedCustomer? Customer { get; set; }

	public List<DetachedLine>? Lines { get; set; }
}

[Validatable(Detached = true)]
public partial class DetachedCustomer
{
	[Required]
	public string? Name { get; set; }
}

[Validatable(Detached = true)]
public partial class DetachedLine
{
	[Required]
	public string? Code { get; set; }
}

// Regular parent with a detached child
[Validatable]
public partial class AttachedParentOfDetached
{
	public DetachedCustomer? Customer { get; set; }
}

public class DetachedValidatableObjectTests
{
	[Fact]
	public void DetachedObject_DoesNotImplementValidlyInterfaces()
	{
		Assert.Empty(
			typeof(DetachedOrder).GetInterfaces().Where(i => i.Assembly == typeof(IValidatable).Assembly)
		);
	}

	[Fact]
	public void DetachedObject_IsValidated_IncludingNestedAndCollectionItems()
	{
		var order = new DetachedOrder
		{
			Number = "1",
			Customer = new DetachedCustomer(),
			Lines = [new DetachedLine { Code = "A" }, new DetachedLine()],
		};

		using var result = order.Validate();

		Assert.Equal(
			["Customer/Name", "Lines/1/Code", "Number"],
			result.Properties.Where(p => !p.IsSuccess).Select(p => p.PropertyPath).OrderBy(p => p)
		);
	}

	[Fact]
	public async Task RegularParent_ValidatesDetachedChild()
	{
		using var result = await new AttachedParentOfDetached { Customer = new DetachedCustomer() }.ValidateAsync();

		Assert.Equal("Customer/Name", Assert.Single(result.Properties, p => !p.IsSuccess).PropertyPath);
	}

#if NET5_0_OR_GREATER
	[Fact]
	public async Task DetachedObject_IsRegistered_ValidatableByRuntimeType()
	{
		object order = new DetachedOrder { Number = "12345" };

		Assert.True(ValidatorRegistry.TryValidate(order, null, default, out var resultTask));

		using var result = await resultTask;
		Assert.True(result.IsSuccess);
	}
#endif

	[Fact]
	public async Task RegularObject_IsValidatableThroughRegistry()
	{
		object obj = new AttachedParentOfDetached();

		Assert.True(ValidatorRegistry.TryValidate(obj, null, default, out var resultTask));

		using var result = await resultTask;
		Assert.True(result.IsSuccess);
	}
}

// Review regressions: detached derived from a regular base, regular derived from a detached base
[Validatable]
public partial class AttachedBaseForDetached
{
	[Required]
	public string? Name { get; set; }
}

[Validatable(Detached = true)]
public partial class DetachedDerivedFromAttached : AttachedBaseForDetached
{
	[Required]
	public string? Extra { get; set; }
}

[Validatable(Detached = true)]
public partial class DetachedBaseForAttached
{
	[Required]
	public string? Name { get; set; }
}

[Validatable]
public partial class AttachedDerivedFromDetached : DetachedBaseForAttached
{
	[Required]
	public string? Extra { get; set; }
}

[Validatable]
public partial class ParentOfMixedHierarchies
{
	public AttachedBaseForDetached? First { get; set; }

	public DetachedBaseForAttached? Second { get; set; }

	public List<AttachedBaseForDetached>? Items { get; set; }
}

public class DetachedHierarchyTests
{
	[Fact]
	public async Task NestedDerivedObjects_AreValidatedPolymorphically()
	{
		using var result = await new ParentOfMixedHierarchies
		{
			First = new DetachedDerivedFromAttached(),
			Second = new AttachedDerivedFromDetached(),
			Items = [new DetachedDerivedFromAttached()],
		}.ValidateAsync();

		Assert.Equal(
			["First/Extra", "First/Name", "Items/0/Extra", "Items/0/Name", "Second/Extra", "Second/Name"],
			result.Properties.Where(p => !p.IsSuccess).Select(p => p.PropertyPath).OrderBy(p => p, StringComparer.Ordinal)
		);
	}
}
