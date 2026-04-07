using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks.Dev;

[SimpleJob(RuntimeMoniker.Net10_0, baseline: true)]
// [SimpleJob(RuntimeMoniker.NativeAot10_0)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
public class DevValidlyBenchmark
{
	public static bool EnableOneTest { get; set; }

	[ParamsSource(nameof(Objects))]
	public CreateUserRequest NumberOfInvalidValues { get; set; } = null!;

	public IEnumerable<CreateUserRequest> Objects =>
		new CreateUserRequest?[]
		{
			new()
			{
				Username = "username",
				Password = "S0m3_pa55w0rd#",
				Email = "email@gmail.com",
				Age = 25,
				FirstName = "Tony",
				LastName = "Stark",
				NumberOfInvalidItems = "none",
			},
			EnableOneTest
				? new()
				{
					Username = "",
					Password = "S0m3_pa55w0rd#",
					Email = "email@gmail.com",
					Age = 25,
					FirstName = "Tony",
					LastName = "Stark",
					NumberOfInvalidItems = "one",
				}
				: null,
			new()
			{
				Username = "Tom",
				Password = "pass",
				Email = "email[at]gmail.com",
				Age = 16,
				FirstName = "",
				LastName = "",
				NumberOfInvalidItems = "all",
			},
		}.Where(x => x != null)!;

	[Benchmark]
	public bool Validly()
	{
		using var result = NumberOfInvalidValues.Validate();
		return result.IsSuccess;
	}
}
