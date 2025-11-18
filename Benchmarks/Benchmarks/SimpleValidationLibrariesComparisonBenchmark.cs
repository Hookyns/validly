using System.ComponentModel.DataAnnotations;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Validot;
using Validator = System.ComponentModel.DataAnnotations.Validator;

namespace Benchmarks;

// [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
// [SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net90)]
// [SimpleJob(RuntimeMoniker.NativeAot80)]
[MaxIterationCount(20)]
// [RPlotExporter]
[MemoryDiagnoser]
public class SimpleValidationLibrariesComparisonBenchmark
{
	[ParamsSource(nameof(Objects))]
	public CreateUserRequest NumberOfInvalidValues { get; set; } = null!;

	public IEnumerable<CreateUserRequest> Objects =>
		new CreateUserRequest[]
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
			new()
			{
				Username = "",
				Password = "S0m3_pa55w0rd#",
				Email = "email@gmail.com",
				Age = 25,
				FirstName = "Tony",
				LastName = "Stark",
				NumberOfInvalidItems = "one",
			},
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
		};

	// [GlobalSetup]
	// public void Setup() { }

	[Benchmark(Baseline = true)]
	public bool DataAnnotation() =>
		Validator.TryValidateObject(NumberOfInvalidValues, new ValidationContext(NumberOfInvalidValues), null);

	private static readonly IValidator<CreateUserRequest> ValidotValidator =
		Validot.Validator.Factory.Create<CreateUserRequest>(_ =>
			_.Member(m => m.Username, m => m.Required().And().LengthBetween(5, 20))
				.Member(m => m.Password, m => m.Required().And().MinLength(12))
				.Member(m => m.Email, m => m.Required().And().Email(EmailValidationMode.DataAnnotationsCompatible))
				.Member(m => m.Age, m => m.BetweenOrEqualTo(18, 99))
				.Member(m => m.FirstName, m => m.Optional().NotEmpty())
				.Member(m => m.LastName, m => m.Optional().NotEmpty())
		);

	[Benchmark]
	public bool Validly()
	{
		using var result = NumberOfInvalidValues.Validate();
		return result.IsSuccess;
	}

	// [Benchmark(Description = "Validly (exit-early)")]
	// public bool ValidlyExitEarly()
	// {
	// 	using var result = NumberOfInvalidValuesExitEarly.Validate();
	// 	return result.IsSuccess;
	// }

	[Benchmark(Description = "Validot (IsValid)")]
	public bool ValidotIsValid()
	{
		return ValidotValidator.IsValid(NumberOfInvalidValues);
	}

	[Benchmark(Description = "Validot (Validate)")]
	public bool ValidotValidate()
	{
		return !ValidotValidator.Validate(NumberOfInvalidValues).AnyErrors;
	}

	private static readonly CreateUserRequestValidator FluentValidator = new();

	[Benchmark]
	public bool FluentValidation()
	{
		return FluentValidator.Validate(NumberOfInvalidValues).IsValid;
	}
}
