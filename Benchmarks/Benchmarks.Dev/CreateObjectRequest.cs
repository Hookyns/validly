using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Validly;
using Validly.Validators;

namespace Benchmarks.Dev;

[Validatable]
public partial record CreateObjectRequest
{
	[CustomValidation]
	public string? IntegrationField { get; set; }

	public async IAsyncEnumerable<ValidationMessage> ValidateIntegrationField(
		[FromKeyedServices("asdas")]IExternalService externalService,
		[EnumeratorCancellation] CancellationToken ct = default)
	{
		if (IntegrationField is null)
			yield break;
		var isValid = await externalService.IsValidValue(IntegrationField, ct);
		if (!isValid)
			yield return new ValidationMessage("Integration field is invalid", "External.Integration");
	}
}