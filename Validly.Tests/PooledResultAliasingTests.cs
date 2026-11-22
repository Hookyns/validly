using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Strings;

namespace Validly.Tests;

[Validatable]
public partial class PoolingLeaf
{
	[Required]
	public string? Value { get; set; }
}

[Validatable]
public partial class PoolingParent
{
	public PoolingLeaf Nested { get; set; } = new();
}

[Validatable]
public partial class PoolingTwoChildrenParent
{
	public PoolingLeaf First { get; set; } = new();

	public PoolingLeaf Second { get; set; } = new();
}

[Validatable]
public partial class PoolingNullableParent
{
	public PoolingLeaf? Nested { get; set; }
}

[Validatable]
public partial class PoolingUnrelated
{
	[MinLength(5)]
	public string? Name { get; set; }
}

[Validatable]
public partial record PoolingRecordParent
{
	public PoolingRecordLeaf Nested { get; init; } = new();
}

[Validatable]
public partial record PoolingRecordLeaf
{
	[Required]
	public string? Value { get; init; }
}

/// <summary>
/// Results are pooled; a result must stay intact until it is disposed, no matter what is validated meanwhile
/// </summary>
public class PooledResultAliasingTests
{
	[Fact]
	public async Task NestedResult_KeepsPropertyPath_WhenAnotherObjectIsValidated()
	{
		using var parentResult = await new PoolingParent().ValidateAsync();
		Assert.Equal("Nested/Value", parentResult.Properties.Single().PropertyPath);

		using var unrelatedResult = new PoolingUnrelated { Name = "valid name" }.Validate();

		Assert.Equal("Nested/Value", parentResult.Properties.Single().PropertyPath);
	}

	[Fact]
	public async Task NestedResult_KeepsMessages_WhenAnotherObjectIsValidated()
	{
		using var parentResult = await new PoolingParent().ValidateAsync();
		var message = parentResult.Properties.Single().Messages.Single();

		using var unrelatedResult = new PoolingUnrelated { Name = "x" }.Validate();

		Assert.False(parentResult.IsSuccess);
		Assert.Same(message, parentResult.Properties.Single().Messages.Single());
	}

	[Fact]
	public async Task TwoResultsAliveAtOnce_AreIndependent_AfterNestedValidation()
	{
		// Nested validation that is already finished and disposed
		(await new PoolingParent().ValidateAsync()).Dispose();

		using var validResult = new PoolingLeaf { Value = "value" }.Validate();
		using var invalidResult = new PoolingLeaf().Validate();

		Assert.True(validResult.IsSuccess);
		Assert.False(invalidResult.IsSuccess);
	}

	[Fact]
	public async Task TwoValidatableChildren_DoNotOverflowTheResult()
	{
		for (int i = 0; i < 100; i++)
		{
			using var result = await new PoolingTwoChildrenParent().ValidateAsync();

			Assert.Equal(["First/Value", "Second/Value"], result.Properties.Select(p => p.PropertyPath));
			Assert.All(result.Properties, p => Assert.Single(p.Messages));
		}
	}

	[Fact]
	public async Task NullNestedObject_IsSkipped()
	{
		using var result = await new PoolingNullableParent { Nested = null }.ValidateAsync();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task NestedObject_ValidatedStandaloneAfterParent_CreatesItsOwnContext()
	{
		var leaf = new PoolingLeaf();
		var parent = new PoolingParent { Nested = leaf };

		(await parent.ValidateAsync()).Dispose();

		// The context of the parent validation must not be reused (it is returned to the pool already)
		Assert.Null(((Validly.Validators.IInternalValidationInvoker)leaf).GetValidationContext());

		using var result = leaf.Validate();
		Assert.Equal("Value", result.Properties.Single().PropertyPath);
	}

	[Fact]
	public async Task RecordEquality_IsNotAffectedByValidation()
	{
		var first = new PoolingRecordParent();
		var second = new PoolingRecordParent { Nested = first.Nested };

		(await first.ValidateAsync()).Dispose();

		Assert.Equal(first, second);
		Assert.Equal(first.GetHashCode(), second.GetHashCode());
	}

	[Fact]
	public async Task ParallelNestedValidations_DoNotLeakPathsOrMessages()
	{
		await Task.WhenAll(
			Enumerable
				.Range(0, Environment.ProcessorCount * 2)
				.Select(worker =>
					Task.Run(async () =>
					{
						for (int i = 0; i < 2_000; i++)
						{
							if ((i + worker) % 3 == 0)
							{
								using var unrelated = new PoolingUnrelated { Name = "x" }.Validate();
								Assert.Equal("Name", unrelated.Properties.Single().PropertyPath);
								Assert.False(unrelated.IsSuccess);
								continue;
							}

							using var result = await new PoolingTwoChildrenParent
							{
								Second = new PoolingLeaf { Value = "ok" },
							}.ValidateAsync();

							Assert.False(result.IsSuccess);
							Assert.Equal(
								["First/Value", "Second/Value"],
								result.Properties.Select(p => p.PropertyPath)
							);
							Assert.Single(result.Properties[0].Messages);
							Assert.Empty(result.Properties[1].Messages);
						}
					})
				)
		);
	}

	[Fact]
	public void PropertyResult_DoesNotOverflowIntoNextProperty()
	{
		using var result = ExtendableValidationResult.Create(2);
		var internalResult = (IInternalValidationResult)result;

		var first = internalResult.InitProperty("First");
		internalResult.InitProperty("Second");

		for (int i = 0; i < ValidlyOptions.PropertyMessagesPoolSize; i++)
		{
			first.Add(new ValidationMessage($"first {i}", "first"));
		}

		Assert.ThrowsAny<Exception>(() => first.Add(new ValidationMessage("overflow", "first")));
		Assert.Empty(result.Properties[1].Messages);
	}
}
