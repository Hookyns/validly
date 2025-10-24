using Validly.Validators;

namespace Validly.SourceGenerator;

internal static class Consts
{
	public const string CustomValidationAttribute =
		$"Validly.Validators.{nameof(Validly.Validators.CustomValidationAttribute)}";

	public const string RequiredAttributeQualifiedName = "Validly.Extensions.Validators.Common.RequiredAttribute";
	public const string InEnumAttributeQualifiedName = "Validly.Extensions.Validators.Enums.InEnumAttribute";
	public const string ValidatableAttributeQualifiedName = $"Validly.{nameof(ValidatableAttribute)}";
	public const string ValidatorAttributeQualifiedName = $"Validly.Validators.{nameof(ValidatorAttribute)}";
	public const string ValidationContextQualifiedName = $"Validly.{nameof(ValidationContext)}";
	public const string ValidationResultQualifiedName = $"Validly.{nameof(ValidationResult)}";
	public const string ExtendableValidationResultQualifiedName = $"Validly.{nameof(ExtendableValidationResult)}";

	public const string InternalValidationInvokerGlobalRef =
		$"global::Validly.Validators.{nameof(IInternalValidationInvoker)}";

	public const string IValidatableGlobalRef = $"global::Validly.{nameof(IValidatable)}";
	public const string ValidationResultGlobalRef = $"global::Validly.{nameof(ValidationResult)}";
	public const string ExpandablePropertyValidationResultGlobalRef =
		$"global::Validly.{nameof(IExpandablePropertyValidationResult)}";
	public const string ValidationContextGlobalRef = $"global::Validly.{nameof(ValidationContext)}";
	public const string ExtendableValidationResultGlobalRef = $"global::Validly.{nameof(ExtendableValidationResult)}";
	public const string InternalValidationResultGlobalRef = $"global::Validly.{nameof(IInternalValidationResult)}";
	public const string ServiceProviderGlobalRef = $"global::System.{nameof(IServiceProvider)}";
	public const string ValidationMessageGlobalRef = $"global::Validly.{nameof(ValidationMessage)}";

	public const string CancellationTokenName = $"{nameof(CancellationToken)}";

	public const string ValidationResultName = "ValidationResult";
	public const string ExtendableValidationResultName = "ExtendableValidationResult";
	public const string CustomValidationMethodPrefix = "Validate";
	public const string ValidationContextName = "ValidationContext";
	public const string ValidationMessageName = "ValidationMessage";
	public const string IsValidMethodName = "IsValid";
	public const string BeforeValidateMethodName = "BeforeValidate";
	public const string AfterValidateMethodName = "AfterValidate";
	public const string FromKeyedServicesAttributeName = "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";
}
