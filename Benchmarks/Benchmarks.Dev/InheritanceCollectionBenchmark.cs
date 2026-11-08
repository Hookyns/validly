using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Validly;
using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Strings;

namespace Benchmarks.Dev;

/// <summary>
/// Inheritance (#29) and collection items (#32); not supported by the previous version
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class InheritanceCollectionBenchmark
{
	private readonly BenchSixFlat _sixFlat = new()
	{
		A = "valid",
		B = "valid",
		C = "valid",
		D = "valid",
		E = "valid",
		F = "valid",
	};

	private readonly BenchDerived _derived = new()
	{
		A = "valid",
		B = "valid",
		C = "valid",
		D = "valid",
		E = "valid",
		F = "valid",
	};

	private readonly BenchDerived _derivedInvalid = new()
	{
		A = "x",
		B = "valid",
		C = "valid",
		D = "x",
		E = "valid",
		F = "valid",
	};

	private BenchOrder _order = null!;

	[Params(0, 10, 1000)]
	public int Items { get; set; }

	[Params(false, true)]
	public bool TenPercentInvalid { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_order = new BenchOrder
		{
			Title = "valid title",
			Lines = Enumerable
				.Range(0, Items)
				.Select(i => new BenchLine { Code = TenPercentInvalid && i % 10 == 0 ? null : "code" })
				.ToList(),
		};
	}

	[Benchmark]
	public bool Inheritance_SixPropertiesFlat_Valid()
	{
		using var result = _sixFlat.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Inheritance_ThreePlusThree_Valid()
	{
		using var result = _derived.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Inheritance_ThreePlusThree_Invalid()
	{
		using var result = _derivedInvalid.Validate();
		return result.IsSuccess;
	}

	[Benchmark]
	public bool Collection()
	{
		using var result = _order.ValidateAsync().Result;
		return result.IsSuccess;
	}
}

[Validatable(NoAutoValidators = true)]
public partial class BenchSixFlat
{
	[MinLength(3)]
	public string? A { get; set; }

	[MinLength(3)]
	public string? B { get; set; }

	[MinLength(3)]
	public string? C { get; set; }

	[MinLength(3)]
	public string? D { get; set; }

	[MinLength(3)]
	public string? E { get; set; }

	[MinLength(3)]
	public string? F { get; set; }
}

[Validatable(NoAutoValidators = true)]
public partial class BenchBase
{
	[MinLength(3)]
	public string? A { get; set; }

	[MinLength(3)]
	public string? B { get; set; }

	[MinLength(3)]
	public string? C { get; set; }
}

[Validatable(NoAutoValidators = true)]
public partial class BenchDerived : BenchBase
{
	[MinLength(3)]
	public string? D { get; set; }

	[MinLength(3)]
	public string? E { get; set; }

	[MinLength(3)]
	public string? F { get; set; }
}

[Validatable(NoAutoValidators = true)]
public partial class BenchOrder
{
	[MinLength(3)]
	public string? Title { get; set; }

	public List<BenchLine>? Lines { get; set; }
}

[Validatable(NoAutoValidators = true)]
public partial class BenchLine
{
	[Required]
	public string? Code { get; set; }
}

/// <summary>
/// Call of the base level: direct non-virtual call of a protected method (chosen) vs. cast to an explicitly
/// implemented per-type interface (the original idea)
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class BaseCallDispatchBenchmark
{
	private readonly DispatchDerived _derived = new();

	[Benchmark(Baseline = true)]
	public int DirectBaseCall() => _derived.ViaDirectBaseCall();

	[Benchmark]
	public int PerTypeInterfaceCast() => _derived.ViaInterfaceCast();

	public interface IDispatchBaseInvoker
	{
		int Core(int value);
	}

	public class DispatchBase : IDispatchBaseInvoker
	{
		private int _counter;

		protected int CoreDirect(int value) => _counter += value;

		int IDispatchBaseInvoker.Core(int value) => _counter += value;
	}

	public class DispatchDerived : DispatchBase
	{
		public int ViaDirectBaseCall() => base.CoreDirect(1);

		public int ViaInterfaceCast() => ((IDispatchBaseInvoker)this).Core(1);
	}
}
