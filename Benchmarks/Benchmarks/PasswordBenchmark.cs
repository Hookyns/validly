using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Runtime.CompilerServices;
using Validly;
using Validly.Extensions.Validators.Strings;

namespace Benchmarks;

[SimpleJob(RuntimeMoniker.Net472, baseline: true)]
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[RankColumn]
public class PasswordBenchmark
{
	private const int MinLength = 12;
	private const int NumberOfRequiredSpecialCharacters = 1;
	private const int NumberOfRequiredUpperCaseCharacters = 1;
	private const int NumberOfRequiredDigitCharacters = 1;

	private const string WeakPassword = "abc";
	private const string LongValidPassword = "MySecureP@ssw0rd!2024";
	private const string LongInvalidPassword = "mysecurepasswordwithoutrequirements";

	private readonly PasswordAttribute _validator = new(
		MinLength,
		NumberOfRequiredSpecialCharacters,
		NumberOfRequiredUpperCaseCharacters,
		NumberOfRequiredDigitCharacters);

	[Benchmark(Baseline = true)]
	public ValidationMessage? Foreach_Valid()
		=> _validator.IsValid(LongValidPassword);

	[Benchmark]
	public ValidationMessage? Foreach_Invalid()
		=> _validator.IsValid(LongInvalidPassword);

	[Benchmark]
	public ValidationMessage? Foreach_Short()
		=> _validator.IsValid(WeakPassword);

	[Benchmark]
	public ValidationMessage? Span_Valid()
		=> IsValidSpan(
			LongValidPassword,
			MinLength,
			NumberOfRequiredSpecialCharacters,
			NumberOfRequiredUpperCaseCharacters,
			NumberOfRequiredDigitCharacters);

	[Benchmark]
	public ValidationMessage? Span_Invalid()
		=> IsValidSpan(
			LongInvalidPassword,
			MinLength,
			NumberOfRequiredSpecialCharacters,
			NumberOfRequiredUpperCaseCharacters,
			NumberOfRequiredDigitCharacters);

	[Benchmark]
	public ValidationMessage? Span_Short()
		=> IsValidSpan(
			WeakPassword,
			MinLength,
			NumberOfRequiredSpecialCharacters,
			NumberOfRequiredUpperCaseCharacters,
			NumberOfRequiredDigitCharacters);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ValidationMessage? IsValidSpan(
		string? value,
		int minLength,
		int requiredSpecial,
		int requiredUpper,
		int requiredDigit)
	{
		if (value is null || value.Length < minLength)
		{
			return new ValidationMessage("Password must be at least {0} characters long.", "Length", minLength);
		}

		var span = value.AsSpan();

		var specialCharCount = 0;
		var upperCaseCount = 0;
		var digitCount = 0;

		for (int i = 0; i < span.Length; i++)
		{
			var c = span[i];

			if (requiredSpecial > 0 && !char.IsLetterOrDigit(c))
			{
				specialCharCount++;
			}

			if (requiredUpper > 0 && char.IsUpper(c))
			{
				upperCaseCount++;
			}

			if (requiredDigit > 0 && char.IsDigit(c))
			{
				digitCount++;
			}

			if (specialCharCount >= requiredSpecial
				&& upperCaseCount >= requiredUpper
				&& digitCount >= requiredDigit)
			{
				break;
			}
		}

		if (specialCharCount < requiredSpecial)
		{
			return new(
				"The password must contain at least {0} special character letter(s).",
				$"SpecialChar",
				NumberOfRequiredSpecialCharacters);
		}

		if (upperCaseCount < requiredUpper)
		{
			return new(
				"The password must contain at least {0} uppercase letter(s).",
				"UpperCase",
				NumberOfRequiredUpperCaseCharacters);
		}

		if (digitCount < requiredDigit)
		{
			return new(
				"The password must contain at least {0} digit letter(s).",
				"Digit",
				NumberOfRequiredUpperCaseCharacters);
		}

		return null;
	}
}