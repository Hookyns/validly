namespace Validly.SourceGenerator;

internal static class Consts
{
	public const string CustomValidationAttribute = "Validly.Validators.CustomValidationAttribute";

	public const string RequiredAttributeQualifiedName = "Validly.Extensions.Validators.Common.RequiredAttribute";
	public const string InEnumAttributeQualifiedName = "Validly.Extensions.Validators.Enums.InEnumAttribute";
	public const string ValidatableAttributeQualifiedName = "Validly.ValidatableAttribute";
	public const string ValidatorAttributeQualifiedName = "Validly.Validators.ValidatorAttribute";
	public const string ValidationContextQualifiedName = "Validly.ValidationContext";
	public const string ValidationResultQualifiedName = "Validly.ValidationResult";
	public const string ExtendableValidationResultQualifiedName = "Validly.ExtendableValidationResult";

	public const string InternalValidationInvokerGlobalRef = "global::Validly.Validators.IInternalValidationInvoker";

	public const string IValidatableGlobalRef = "global::Validly.IValidatable";
	public const string ValidationResultGlobalRef = "global::Validly.ValidationResult";
	public const string ExpandablePropertyValidationResultGlobalRef =
		"global::Validly.IExpandablePropertyValidationResult";
	public const string ValidationContextGlobalRef = "global::Validly.ValidationContext";
	public const string ValidationContextDisposerGlobalRef = "global::Validly.ValidationContextDisposer";
	public const string ExtendableValidationResultGlobalRef = "global::Validly.ExtendableValidationResult";
	public const string InternalValidationResultGlobalRef = "global::Validly.IInternalValidationResult";
	public const string ServiceProviderGlobalRef = $"global::System.{nameof(IServiceProvider)}";
	public const string WeakReferenceGlobalRef = $"global::System.{nameof(WeakReference)}";
	public const string ValidationMessageGlobalRef = "global::Validly.ValidationMessage";

	public const string ValidatableAttributeUseAutoValidatorsPropertyName = "UseAutoValidators";
	public const string ValidatableAttributeNoAutoValidatorsPropertyName = "NoAutoValidators";
	public const string ValidatableAttributeUseExitEarlyPropertyName = "UseExitEarly";
	public const string ValidatableAttributeNoAExitEarlyPropertyName = "NoExitEarly";

	public const string CancellationTokenName = $"{nameof(CancellationToken)}";

	public const string ValidationResultName = "ValidationResult";
	public const string ExtendableValidationResultName = "ExtendableValidationResult";
	public const string CustomValidationMethodPrefix = "Validate";
	public const string ValidationContextName = "ValidationContext";
	public const string ValidationMessageName = "ValidationMessage";
	public const string IsValidMethodName = "IsValid";
	public const string BeforeValidateMethodName = "BeforeValidate";
	public const string AfterValidateMethodName = "AfterValidate";
	public const string FromKeyedServicesAttributeName =
		"Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";
}
