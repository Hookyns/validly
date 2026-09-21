using Validly.Extensions.Validators.Common;

namespace Validly.Tests.Validators.Common;

[Validatable(NoAutoValidators = true)]
partial class NotEmptyStringTestObject
{
	[NotEmpty]
	public required string Value { get; init; }
}

[Validatable(NoAutoValidators = true)]
partial class NotEmptyCollectionTestObject
{
	[NotEmpty]
	public required List<int> Values { get; init; }
}

readonly record struct StronglyTypedId(Guid Value)
{
	public static readonly StronglyTypedId Empty = new(Guid.Empty);
}

[Validatable(NoAutoValidators = true)]
partial class ObjectWithStronglyTypedId
{
	[NotEmpty]
	public required StronglyTypedId Id { get; init; }
}

[Validatable(NoAutoValidators = true)]
partial class ObjectWithNullableStronglyTypedId
{
	[NotEmpty]
	public required StronglyTypedId? Id { get; init; }
}

[Validatable(NoAutoValidators = true)]
partial class ObjectWithStructCollection
{
	[NotEmpty]
	public required ArraySegment<int> Values { get; init; }
}

[Validatable(NoAutoValidators = true)]
partial class NotEmptyEnumerableTestObject
{
	[NotEmpty]
	public required IEnumerable<int> Values { get; init; }
}

public class NotEmptyTests
{
	[Fact]
	public void StringValue_IsValid()
	{
		var result = new NotEmptyStringTestObject { Value = "test" }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void StringValue_IsInvalid()
	{
		var result = new NotEmptyStringTestObject { Value = "" }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void Collection_IsValid()
	{
		var result = new NotEmptyCollectionTestObject
		{
			Values = new List<int> { 1, 2, 3 },
		}.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void Collection_IsInvalid()
	{
		var result = new NotEmptyCollectionTestObject { Values = new List<int>() }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void Enumerable_IsValid()
	{
		var result = new NotEmptyEnumerableTestObject { Values = new List<int> { 1, 2, 3 }.AsEnumerable() }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void Enumerable_IsInvalid()
	{
		var result = new NotEmptyEnumerableTestObject { Values = new List<int>().AsEnumerable() }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void EnumerableList_IsValid()
	{
		var result = new NotEmptyEnumerableTestObject
		{
			Values = new List<int> { 1, 2, 3 },
		}.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void EnumerableList_IsInvalid()
	{
		var result = new NotEmptyEnumerableTestObject { Values = new List<int>() }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void NullEnumerable_IsValid()
	{
		var result = new NotEmptyEnumerableTestObject { Values = null! }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void StronglyTypedId_IsValid()
	{
		var result = new ObjectWithStronglyTypedId { Id = new StronglyTypedId(Guid.NewGuid()) }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void StronglyTypedId_IsInvalid()
	{
		var result = new ObjectWithStronglyTypedId { Id = default }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void EmptyStronglyTypedId_IsInvalid()
	{
		var result = new ObjectWithStronglyTypedId { Id = StronglyTypedId.Empty }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void NullableStronglyTypedId_IsValid()
	{
		var result = new ObjectWithNullableStronglyTypedId { Id = new StronglyTypedId(Guid.NewGuid()) }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void NullableStronglyTypedId_IsInvalid()
	{
		var result = new ObjectWithNullableStronglyTypedId { Id = default(StronglyTypedId) }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void NullStronglyTypedId_IsValid()
	{
		var result = new ObjectWithNullableStronglyTypedId { Id = null }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void StructCollection_IsValid()
	{
		var result = new ObjectWithStructCollection { Values = new ArraySegment<int>(new[] { 1, 2, 3 }) }.Validate();
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void StructCollection_IsInvalid()
	{
		var result = new ObjectWithStructCollection { Values = new ArraySegment<int>(Array.Empty<int>()) }.Validate();
		Assert.False(result.IsSuccess);
	}

	[Fact]
	public void DefaultStructCollection_IsInvalid()
	{
		var result = new ObjectWithStructCollection { Values = default }.Validate();
		Assert.False(result.IsSuccess);
	}
}
