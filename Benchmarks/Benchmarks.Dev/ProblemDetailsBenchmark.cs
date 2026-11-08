using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks.Dev;

/// <summary>
/// Benchmark of the validation and of the problem details generation, including nested objects.
/// </summary>
/// <remarks>
/// Uses only the parameterless problem details API, so the very same benchmark can be run against older versions.
/// </remarks>
[SimpleJob(RuntimeMoniker.Net10_0, baseline: true)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class ProblemDetailsBenchmark
{
	private static readonly CreateUserRequest ValidUser = new()
	{
		Username = "username",
		Password = "S0m3_pa55w0rd#",
		Email = "email@gmail.com",
		Age = 25,
		FirstName = "Tony",
		LastName = "Stark",
		NumberOfInvalidItems = "none",
	};

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

	[Benchmark]
	public bool Flat_Validate_Valid()
	{
		using var result = ValidUser.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Flat_Validate_Invalid()
	{
		using var result = InvalidUser.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public int Flat_ProblemDetailsJson()
	{
		using var result = InvalidUser.Validate();
		return result.GetProblemDetailsJson().Length;
	}

	[Benchmark]
	public int Flat_ProblemDetails()
	{
		using var result = InvalidUser.Validate();
		return result.GetProblemDetails().Errors.Count;
	}

	[Benchmark]
	public async ValueTask<bool> Nested_Validate_Invalid()
	{
		using var result = await InvalidNested.ValidateAsync();
		return result.IsSuccess;
	}

	[Benchmark]
	public async ValueTask<int> Nested_ProblemDetailsJson()
	{
		using var result = await InvalidNested.ValidateAsync();
		return result.GetProblemDetailsJson().Length;
	}
}
