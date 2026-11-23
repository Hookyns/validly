using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Validly.Naming;

namespace Benchmarks.Dev;

/// <summary>
/// Benchmark of the problem details generation with a JSON naming policy applied to the error pointers.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0, baseline: true)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class NamingPolicyBenchmark
{
	private static readonly CreateUserRequest InvalidUser = new()
	{
		Username = "Tom",
		Password = "pass",
		Email = "email[at]gmail.com",
		Age = 16,
		FirstName = "",
		LastName = "",
		NumberOfInvalidItems = "all",
	};

	private static readonly NestedRoot InvalidNested = new()
	{
		RootProp = "x",
		Named = new() { ChildProp = "x", NamedChildProp = "x" },
	};

	[Benchmark(Baseline = true)]
	public int Flat_Json_NoPolicy()
	{
		using var result = InvalidUser.Validate();
		return result.GetProblemDetailsJson().Length;
	}

	[Benchmark]
	public int Flat_Json_CamelCase()
	{
		using var result = InvalidUser.Validate();
		return result.GetProblemDetailsJson(PropertyNamingPolicy.CamelCase).Length;
	}

	[Benchmark]
	public async ValueTask<int> Nested_Json_NoPolicy()
	{
		using var result = await InvalidNested.ValidateAsync();
		return result.GetProblemDetailsJson().Length;
	}

	[Benchmark]
	public async ValueTask<int> Nested_Json_CamelCase()
	{
		using var result = await InvalidNested.ValidateAsync();
		return result.GetProblemDetailsJson(PropertyNamingPolicy.CamelCase).Length;
	}
}
