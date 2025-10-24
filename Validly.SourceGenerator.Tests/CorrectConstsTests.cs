using Validly.Validators;

namespace Validly.SourceGenerator.Tests;

public class CorrectConstsTests
{
	[Fact]
	public void ConstsMatchesTest()
	{
		Assert.Equal(Consts.CustomValidationAttribute, $"Validly.Validators.{nameof(CustomValidationAttribute)}");
		Assert.Equal(Consts.RequiredAttributeQualifiedName, "Validly.Extensions.Validators.Common.RequiredAttribute");
		Assert.Equal(Consts.InEnumAttributeQualifiedName, "Validly.Extensions.Validators.Enums.InEnumAttribute");
		Assert.Equal(Consts.ValidatableAttributeQualifiedName, $"Validly.{nameof(ValidatableAttribute)}");
		Assert.Equal(Consts.ValidatorAttributeQualifiedName, $"Validly.Validators.{nameof(ValidatorAttribute)}");
		Assert.Equal(Consts.ValidationContextQualifiedName, $"Validly.{nameof(ValidationContext)}");
		Assert.Equal(Consts.ValidationResultQualifiedName, $"Validly.{nameof(ValidationResult)}");
		Assert.Equal(Consts.ExtendableValidationResultQualifiedName, $"Validly.{nameof(ExtendableValidationResult)}");
		Assert.Equal(
			Consts.InternalValidationInvokerGlobalRef,
			$"global::Validly.Validators.{nameof(IInternalValidationInvoker)}"
		);
		Assert.Equal(Consts.IValidatableGlobalRef, $"global::Validly.{nameof(IValidatable)}");
		Assert.Equal(Consts.ValidationResultGlobalRef, $"global::Validly.{nameof(ValidationResult)}");
		Assert.Equal(
			Consts.ExpandablePropertyValidationResultGlobalRef,
			$"global::Validly.{nameof(IExpandablePropertyValidationResult)}"
		);
		Assert.Equal(Consts.ValidationContextGlobalRef, $"global::Validly.{nameof(ValidationContext)}");
		Assert.Equal(
			Consts.ExtendableValidationResultGlobalRef,
			$"global::Validly.{nameof(ExtendableValidationResult)}"
		);
		Assert.Equal(Consts.InternalValidationResultGlobalRef, $"global::Validly.{nameof(IInternalValidationResult)}");
		Assert.Equal(Consts.ServiceProviderGlobalRef, $"global::System.{nameof(IServiceProvider)}");
		Assert.Equal(Consts.ValidationMessageGlobalRef, $"global::Validly.{nameof(ValidationMessage)}");
		Assert.Equal(
			Consts.ValidatableAttributeUseAutoValidatorsPropertyName,
			nameof(ValidatableAttribute.UseAutoValidators)
		);
		Assert.Equal(
			Consts.ValidatableAttributeNoAutoValidatorsPropertyName,
			nameof(ValidatableAttribute.NoAutoValidators)
		);
		Assert.Equal(Consts.ValidatableAttributeUseExitEarlyPropertyName, nameof(ValidatableAttribute.UseExitEarly));
		Assert.Equal(Consts.ValidatableAttributeNoAExitEarlyPropertyName, nameof(ValidatableAttribute.NoExitEarly));
	}
}
