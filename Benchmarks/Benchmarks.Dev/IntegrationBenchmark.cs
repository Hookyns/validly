using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Validly;
using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Strings;

namespace Benchmarks.Dev;

/// <summary>
/// Hot paths touched by the 1.x fixes; compiles against the previous version too, so before/after can be compared
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class IntegrationBenchmark
{
	private readonly CreateUserRequest _sealedValid = new()
	{
		Username = "username",
		Password = "S0m3_pa55w0rd#",
		Email = "email@gmail.com",
		Age = 25,
		FirstName = "Tony",
		LastName = "Stark",
		NumberOfInvalidItems = "none",
	};

	private readonly CreateUserRequest _sealedInvalid = new()
	{
		Username = "Tom",
		Password = "pass",
		Email = "email[at]gmail.com",
		Age = 16,
		FirstName = "",
		LastName = "",
		NumberOfInvalidItems = "all",
	};

	private readonly BenchFlat _flatValid = new() { Name = "valid name", Code = "valid code", Note = "valid note" };
	private readonly BenchFlat _flatInvalid = new() { Name = "x", Code = null, Note = "y" };

	private readonly BenchParent _nestedValid = new()
	{
		Title = "valid title",
		Child = new BenchFlat { Name = "valid name", Code = "valid code", Note = "valid note" },
	};

	private readonly BenchParent _nestedInvalid = new()
	{
		Title = "x",
		Child = new BenchFlat { Name = "x", Code = null, Note = "y" },
	};

	[Benchmark]
	public bool SealedFlat_Valid()
	{
		using var result = _sealedValid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool SealedFlat_Invalid()
	{
		using var result = _sealedInvalid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Flat_Valid()
	{
		using var result = _flatValid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Flat_Invalid()
	{
		using var result = _flatInvalid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Nested_Valid()
	{
		using var result = _nestedValid.ValidateAsync().Result;
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Nested_Invalid()
	{
		using var result = _nestedInvalid.ValidateAsync().Result;
		return result.IsSuccess;
	}

	[Benchmark]
	public int Nested_Invalid_ProblemDetailsJson()
	{
		using var result = _nestedInvalid.ValidateAsync().Result;
		return result.GetProblemDetailsJson().Length;
	}
}

[Validatable(NoAutoValidators = true)]
public partial class BenchFlat
{
	[MinLength(5)]
	public string? Name { get; set; }

	[Required]
	public string? Code { get; set; }

	[MinLength(5)]
	public string? Note { get; set; }
}

[Validatable(NoAutoValidators = true)]
public partial class BenchParent
{
	[MinLength(5)]
	public string? Title { get; set; }

	public BenchFlat? Child { get; set; }
}
