using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Benchmarks;

if (args.Any(arg => arg == "inprocess"))
{
	Job job = Job
		.Default.WithMinWarmupCount(2)
		.WithMaxWarmupCount(4)
		.WithMinIterationCount(5)
		.WithMaxIterationCount(10)
		.WithToolchain(InProcessEmitToolchain.Instance);

	IConfig config = DefaultConfig
		.Instance.AddDiagnoser(MemoryDiagnoser.Default)
		.WithOptions(ConfigOptions.DisableOptimizationsValidator)
		.AddJob(job);

	BenchmarkRunner.Run<PasswordBenchmark>(config);

	Console.Read();
}
else
{
	BenchmarkRunner.Run<PasswordBenchmark>();

	Console.Read();
}
