using Validly.Extensions.Validators.Common;
using Validly.Extensions.Validators.Numbers;
using Validly.Extensions.Validators.Strings;

namespace Validly.Extensions.AspNetCore.Example.Users.Dtos;

[Validatable]
public partial class CreateUserRequest
{
	[LengthBetween(5, 20)]
	public required string Username { get; init; }

	[MinLength(12)]
	public required string Password { get; init; }

	[EmailAddress]
	public required string Email { get; init; }

	[Between(18, 120.1)]
	public required int Age { get; init; }

	[NotEmpty]
	public string? FirstName { get; init; }

	[NotEmpty]
	public string? LastName { get; init; }
}
