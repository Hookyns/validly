# Migrating from Validly 1.x to 2.0

## Packages and namespaces
- Reference only the `Validly` package; it contains the runtime, the built-in validators and the source generator.
  Remove `Validly.SourceGenerator` and `Validly.Extensions.Validators` (a leftover `Validly.SourceGenerator` 1.x
  analyzer is removed automatically and reported as VLY2000).
- All built-in validators live in the `Validly.Validators` namespace:
  replace `using Validly.Extensions.Validators.Common;`, `...Strings;`, `...Numbers;`, `...Collections;`, `...Enums;`
  with `using Validly.Validators;`.
- `<CompilerVisibleProperty Include="ValidlyConfiguration" />` is no longer needed; the package adds it.
- `Validly.Extensions.AspNetCore` 2.x requires `Validly` 2.x.

## Generated code
- The generated code is fully qualified; it does not copy the usings of your file anymore.
- Nested objects and collection items are validated through `IInternalValidatable` (explicitly implemented).
  `IInternalValidationInvoker` (and the hidden context field) and `ValidationContextDisposer` were removed.
- A validatable base class compiled with Validly 1.x is not validated as a part of a derived class compiled with
  Validly 2 (VLY0002); rebuild it with Validly 2.
- New analyzers report unsupported shapes as errors (VLY0010–VLY0017). Code that compiled before but silently
  ignored a result (e.g. a hook returning `List<ValidationMessage>`) is now reported.

## Behaviour (also in 1.4)
- Inheritance of validatable classes, validation of collection items (`Lines/1/ProductCode`), JSON names in
  problem details pointers – see the README.

## Error codes (`ResourceKey` → `Code`)
- `ValidationMessage.ResourceKey` is now `ValidationMessage.Code`. `ResourceKey` still exists as an `[Obsolete]` alias.
  The same applies to `ValidationErrorDetail`.
- The constructor parameter is renamed: `new ValidationMessage(message, code, args)`, `Validation.Error(message, code, args)`.
- Built-in validators use new, stable codes: `Validations.{Validator}[.{Reason}]` (see `ValidationCodes`).

  | 1.x | 2.0 |
  |---|---|
  | `Validly.Validations.LengthBetween.Error` | `Validations.LengthBetween` |
  | `Validly.Validations.Password.Error.Length` | `Validations.Password.Length` |
  | `Validly.Validations.Required` | `Validations.Required` |
  | ... | see `LegacyValidationCodes` / `ValidationCodes` |

- The problem details JSON contains `"code"` instead of `"resourceKey"`.
- To keep the 1.x codes and the `"resourceKey"` JSON property (e.g. clients depend on them), set
  `<ValidlyLegacyValidationCodes>true</ValidlyLegacyValidationCodes>` in the project file (an AppContext switch
  `Validly.UseLegacyValidationCodes`, read once per process). The modes are never mixed.

## Named arguments
- `ValidationMessage.NamedArgs` (and `ValidationErrorDetail.NamedArgs`) contains named arguments, e.g. `min`, `max`;
  the problem details JSON contains `"namedArgs": { ... }` when there are any. Positional `Args` are unchanged.
- `ArgsJson` is computed lazily; `NamedArgsJson` is new.

## Fixes with visible effect
- The `Password` validator's digit message reported the required number of upper-case characters instead of digits.
