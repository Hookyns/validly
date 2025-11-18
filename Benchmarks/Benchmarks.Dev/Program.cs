using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Toolchains.NativeAot;
using Benchmarks.Dev;

// Job job = Job
// 	.Default.WithMinWarmupCount(2)
// 	.WithMaxWarmupCount(2)
// 	// .WithMaxWarmupCount(4)
// 	.WithMinIterationCount(5)
// 	.WithMaxIterationCount(5)
// 	// .WithMaxIterationCount(10)
// 	.WithToolchain(InProcessEmitToolchain.Instance);
//
// IConfig config = DefaultConfig
// 	.Instance.AddDiagnoser(MemoryDiagnoser.Default)
// 	.WithOptions(ConfigOptions.DisableOptimizationsValidator)
// 	.AddJob(job);

// .NET 10 AOT, configured manually because Benchmark.NET has an issue with instruction set for .NET AOT
// Job job = Job
// 	.Default.WithMinWarmupCount(2)
// 	.WithMaxWarmupCount(4)
// 	.WithMinIterationCount(5)
// 	// .WithMaxIterationCount(5)
// 	.WithMaxIterationCount(10)
// 	.WithRuntime(NativeAotRuntime.Net10_0)
// 	.WithToolchain(
// 		NativeAotToolchainBuilder.Create().UseNuGet().IlcInstructionSet("avx2").DisplayName(".NET 10 AOT").ToToolchain()
// 	);
//
// IConfig config = DefaultConfig
// 	.Instance.AddDiagnoser(MemoryDiagnoser.Default)
// 	.AddJob(job);

// RUN
BenchmarkRunner.Run<DevValidlyBenchmark>();
// BenchmarkRunner.Run<DevValidlyExitEarlyBenchmark>();
// BenchmarkRunner.Run<DevValidlyBenchmark>(config);
