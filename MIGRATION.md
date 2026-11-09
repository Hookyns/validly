# Migrating from Validly 1.x to 2.0

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
