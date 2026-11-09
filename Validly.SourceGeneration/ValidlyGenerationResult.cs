using Validly.SourceGenerator.Dtos;
using Validly.SourceGenerator.Utils;

namespace Validly.SourceGeneration;

/// <summary>
/// Generated validation source
/// </summary>
/// <param name="HintName">Suggested hint name for AddSource</param>
/// <param name="SourceText">The generated C# source; a self-contained file (fully qualified names, no usings needed)</param>
public sealed record ValidlyGenerationResult(string HintName, string SourceText);

/// <summary>
/// Validators available to a compilation; equatable, so it can flow through incremental generator pipelines
/// </summary>
public sealed class ValidlyValidatorCatalog : IEquatable<ValidlyValidatorCatalog>
{
	internal ValidlyValidatorCatalog(EquatableArray<ValidatorProperties> validators)
	{
		Validators = validators;
	}

	internal EquatableArray<ValidatorProperties> Validators { get; }

	/// <summary>
	/// Number of the validators
	/// </summary>
	public int Count => Validators.GetArray()?.Length ?? 0;

	/// <inheritdoc />
	public bool Equals(ValidlyValidatorCatalog? other) => other is not null && Validators.Equals(other.Validators);

	/// <inheritdoc />
	public override bool Equals(object? obj) => Equals(obj as ValidlyValidatorCatalog);

	/// <inheritdoc />
	public override int GetHashCode() => Validators.GetHashCode();
}
