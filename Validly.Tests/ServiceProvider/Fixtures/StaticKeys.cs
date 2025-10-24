using Microsoft.Extensions.DependencyInjection;
using Validly.Validators;

namespace Validly.Tests.ServiceProvider.Fixtures;

internal static class StaticKeys
{
	public enum KeysEnum
	{
		Key1,
		Key2,
	}

	public const KeysEnum EnumKey = KeysEnum.Key1;
	public const string StringKey = "StringKey";
	public const char CharKey = 'A';
	public const string UnknownStringKey = "UnknownStringKey";
	public const int IntKey = 123;
	public const bool BoolKey = true;

	public static IValidatable? GetValidatable(object key)
	{
		var stringKeyValue = key.ToString() ?? string.Empty;
		return key switch
		{
			string => new ValidatableObjectStringKeyed(stringKeyValue),
			int => new ValidatableObjectIntKeyed(stringKeyValue),
			KeysEnum => new ValidatableObjectEnumKeyed(stringKeyValue),
			bool => new ValidatableObjectBoolKeyed(stringKeyValue),
			char => new ValidatableObjectCharKeyed(stringKeyValue),
			_ => null,
		};
	}
}

[Validatable]
internal partial record ValidatableObjectEnumKeyed([property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(
		[FromKeyedServices(StaticKeys.EnumKey)] IDependency? dependency
	)
	{
		if (dependency is null)
		{
			yield return new ValidationMessage(
				$"Dependency cannot be null for property {Property}",
				"Dependency.NotNull"
			);
		}
	}
}

[Validatable]
internal partial record ValidatableObjectIntKeyed([property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(
		[FromKeyedServices(StaticKeys.IntKey)] IDependency? dependency
	)
	{
		if (dependency is null)
		{
			yield return new ValidationMessage(
				$"Dependency cannot be null for property {Property}",
				"Dependency.NotNull"
			);
		}
	}
}

[Validatable]
internal partial record ValidatableObjectStringKeyed([property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(
		[FromKeyedServices(StaticKeys.StringKey)] IDependency? dependency
	)
	{
		if (dependency is null)
		{
			yield return new ValidationMessage(
				$"Dependency cannot be null for property {Property}",
				"Dependency.NotNull"
			);
		}
	}
}

[Validatable]
internal partial record ValidatableObjectCharKeyed([property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(
		[FromKeyedServices(StaticKeys.CharKey)] IDependency? dependency
	)
	{
		if (dependency is null)
		{
			yield return new ValidationMessage(
				$"Dependency cannot be null for property {Property}",
				"Dependency.NotNull"
			);
		}
	}
}

[Validatable]
internal partial record ValidatableObjectBoolKeyed([property: CustomValidation] string Property)
{
	public IEnumerable<ValidationMessage> ValidateProperty(
		[FromKeyedServices(StaticKeys.BoolKey)] IDependency? dependency
	)
	{
		if (dependency is null)
		{
			yield return new ValidationMessage(
				$"Dependency cannot be null for property {Property}",
				"Dependency.NotNull"
			);
		}
	}
}
