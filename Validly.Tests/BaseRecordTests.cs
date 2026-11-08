using Validly.Validators;

namespace Validly.Tests;

[Validatable(UseAutoValidators = true)]
public partial record CreateUserRecordRequest
{
	[Required]
	[MinLength(2)]
	public required string Name { get; set; }

	[EmailAddress]
	[CustomValidation]
	public required string Email { get; set; }

	[Required]
	[Between(18, 99)]
	public int Age { get; set; }

	IEnumerable<ValidationMessage> ICreateUserRecordRequestCustomValidation.ValidateEmail()
	{
		return [];
	}

	private ValidationResult AfterValidate(ValidationResult result)
	{
		return result;
	}
}

public class BaseRecordTests
{
	[Fact]
	public void ValidateTest()
	{
		var request = new CreateUserRecordRequest
		{
			Name = "John",
			Email = "john@gmail.com",
			Age = 25,
		};

		using var result = request.Validate();

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public void NameFailTest()
	{
		var request = new CreateUserRecordRequest
		{
			Name = "J",
			Email = "john@gmail.com",
			Age = 25,
		};

		using var result = request.Validate();

		// Validation FAILED
		Assert.False(result.IsSuccess);

		// Asset SINGLE error message on Name property
		var nameResult = result.Properties.SingleOrDefault(x => !x.IsSuccess);
		Assert.NotNull(nameResult);
		Assert.Equal(nameof(CreateUserRecordRequest.Name), nameResult.PropertyPath);
		Assert.Single(nameResult.Messages);

		// Problem detail
		Assert.Single(result.GetProblemDetails().Errors);
	}

	[Fact]
	public void AgeFailTest()
	{
		var request = new CreateUserRecordRequest
		{
			Name = "John",
			Email = "john@gmail.com",
			Age = 16,
		};

		using var result = request.Validate();

		// Validation FAILED
		Assert.False(result.IsSuccess);

		// Asset SINGLE error message on Name property
		var ageResult = result.Properties.SingleOrDefault(x => !x.IsSuccess);
		Assert.NotNull(ageResult);
		Assert.Equal(nameof(CreateUserRecordRequest.Age), ageResult.PropertyPath);
		Assert.Single(ageResult.Messages);

		// Problem detail
		Assert.Single(result.GetProblemDetails().Errors);
	}

	[Fact]
	public void EmailFailTest()
	{
		var request = new CreateUserRecordRequest
		{
			Name = "John",
			Email = "email:john[at]gmail.com",
			Age = 25,
		};

		using var result = request.Validate();

		// Validation FAILED
		Assert.False(result.IsSuccess);

		// Asset SINGLE error message on Name property
		var emailResult = result.Properties.SingleOrDefault(x => !x.IsSuccess);
		Assert.NotNull(emailResult);
		Assert.Equal(nameof(CreateUserRecordRequest.Email), emailResult.PropertyPath);
		Assert.Single(emailResult.Messages);

		// Problem detail
		Assert.Single(result.GetProblemDetails().Errors);
	}
}
