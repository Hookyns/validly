namespace Validly.SourceGenerator.Utils.Mapping;

[Flags]
internal enum ReturnTypeType : short
{
	None = 0,

	Nullable = 1,

	Generic = 1 << 1,

	Void = 1 << 2,

	Task = 1 << 3,

	ValueTask = 1 << 4,

	Enumerable = 1 << 5,

	AsyncEnumerable = 1 << 6,

	ValidationResult = 1 << 7,

	ValidationMessage = 1 << 8,

	Validation = 1 << 9,

	TaskOrValueTask = Task | ValueTask,

	Async = Task | ValueTask,

	Awaitable = Async | AsyncEnumerable,

	MayBeGeneric = Task | ValueTask | Enumerable | AsyncEnumerable,
}
