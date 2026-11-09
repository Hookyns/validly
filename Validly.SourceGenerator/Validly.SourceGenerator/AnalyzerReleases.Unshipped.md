; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
VLY0002 | Validly | Warning | Base class validated by an older version of Validly
VLY0010 | Validly | Error | Validatable type must be partial
VLY0011 | Validly | Error | Validatable type must not be nested
VLY0012 | Validly | Error | Unsupported return type of a validation hook
VLY0013 | Validly | Error | Unsupported parameter of a validation method
VLY0014 | Validly | Error | Missing custom validation method
VLY0015 | Validly | Error | Custom validation method must be public
VLY0016 | Validly | Error | Unsupported return type of a validation method
VLY0017 | Validly | Error | Validator without IsValid method
VLY0100 | Validly | Warning | Unknown JsonNamingPolicy in ValidlyConfiguration
