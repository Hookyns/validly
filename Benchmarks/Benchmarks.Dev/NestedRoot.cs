using System.Text.Json.Serialization;
using Validly;
using Validly.Extensions.Validators.Strings;

namespace Benchmarks.Dev;

[Validatable(NoAutoValidators = true)]
public sealed partial class NestedRoot
{
	[MinLength(5)]
	public required string RootProp { get; set; }

	// NOTE: only one validatable child; two of them hit an existing pooling bug ("Collection is full")
	[JsonPropertyName("named_child")]
	public required NestedChild Named { get; set; }
}

[Validatable(NoAutoValidators = true)]
public sealed partial class NestedChild
{
	[MinLength(5)]
	public required string ChildProp { get; set; }

	[MinLength(5)]
	[JsonPropertyName("named_prop")]
	public required string NamedChildProp { get; set; }
}
