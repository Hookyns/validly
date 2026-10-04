using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Validly;
using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Numbers;
using Validly.Extensions.Validators.Strings;

namespace Benchmarks.Dev;

[Validatable]
public partial class PagedRequest
{
	[Required]
	[LengthBetween(2, 20)]
	public string? Query { get; set; }

	[Between(1, 100)]
	public int PageSize { get; set; }

	[Required]
	public string? Cursor { get; set; }
}

[Validatable]
public partial class PagedRequest<TCursor>
{
	[Required]
	[LengthBetween(2, 20)]
	public string? Query { get; set; }

	[Between(1, 100)]
	public int PageSize { get; set; }

	[Required]
	public TCursor? Cursor { get; set; }
}

/// <summary>
/// Compares validation of a generic object with the same non-generic object
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class GenericObjectBenchmark
{
	private readonly PagedRequest _valid = new()
	{
		Query = "validly",
		PageSize = 20,
		Cursor = "cursor",
	};

	private readonly PagedRequest _invalid = new() { Query = "v", PageSize = 500 };

	[Benchmark]
	public bool NonGeneric_Valid()
	{
		using var result = _valid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool NonGeneric_Invalid()
	{
		using var result = _invalid.Validate();
		return result.IsSuccess;
	}

	private readonly PagedRequest<string> _genericValid = new()
	{
		Query = "validly",
		PageSize = 20,
		Cursor = "cursor",
	};

	private readonly PagedRequest<string> _genericInvalid = new() { Query = "v", PageSize = 500 };

	[Benchmark]
	public bool Generic_Valid()
	{
		using var result = _genericValid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Generic_Invalid()
	{
		using var result = _genericInvalid.Validate();
		return result.IsSuccess;
	}
}
