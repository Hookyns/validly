using System.ComponentModel;
using DDDValidations.Domain.Users.Entities;
using Validly;
using Validly.Validators;

namespace DDDValidations.Domain.Users.Dtos;

[Validatable]
public partial class CreateUserArgs
{
	[DisplayName("PropNames.User.Username")]
	[LengthBetween(5, 20)]
	public required string Username { get; set; }

	[MinLength(12)]
	public required string Password { get; set; }

	[EmailAddress]
	public required string Email { get; set; }

	[Between(18, 99)]
	public required int Age { get; set; }

	public required UserType Type { get; init; }

	[NotEmpty]
	public string? FirstName { get; set; }

	[NotEmpty]
	public string? LastName { get; set; }
}
