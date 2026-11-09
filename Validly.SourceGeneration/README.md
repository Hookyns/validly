# Validly.SourceGeneration

API of the [Validly](https://github.com/Hookyns/validly) source generator for **third-party source generators**.

Source generators cannot see the output of other generators. A generator that produces, for example, DTOs from
entities cannot rely on Validly generating the validation of those DTOs. Instead, it asks this API for the
validation source of a type and adds it to its own output.

```csharp
[Generator]
public sealed class DtoGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var validators = ValidlySourceGeneration.GetValidators(context);
        var entities = context.SyntaxProvider.ForAttributeWithMetadataName(
            "My.GenerateDtoAttribute", static (n, _) => true, static (c, _) => (INamedTypeSymbol)c.TargetSymbol);

        context.RegisterSourceOutput(entities.Combine(validators).Combine(context.CompilationProvider), static (spc, input) =>
        {
            var ((entity, catalog), compilation) = input;

            // ... emit "public partial class {entity.Name}Dto { properties }" ...

            var validation = ValidlySourceGeneration.Generate(entity, compilation, new ValidlyGenerationOptions
            {
                TypeName = entity.Name + "Dto",
                PropertyMap = name => name == "Id" ? null : name, // exclude/rename properties
            }, catalog);

            spc.AddSource(validation.HintName, validation.SourceText);
        });
    }
}
```

- The generated source is a self-contained partial part of the target type (fully qualified names, no usings).
  The target type does not need the `[Validatable]` attribute.
- The project that uses the generator must reference the `Validly` package; the generated code uses its runtime.
- `IncludeBaseProperties` copies the rules of the entity's base classes. Hooks and `[CustomValidation]` are not
  copied by default (`CopyHooks`, `CopyCustomValidation`).

## Shipping it inside your generator
The API assembly must be packed next to your generator in the analyzers folder:

```xml
<ItemGroup>
  <PackageReference Include="Validly.SourceGeneration" Version="2.*" PrivateAssets="all" GeneratePathProperty="true" />
  <None Include="$(PkgValidly_SourceGeneration)\lib\netstandard2.0\Validly.SourceGeneration.dll"
        Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
</ItemGroup>
<!-- Lets a ProjectReference with OutputItemType="Analyzer" load the dependency too -->
<Target Name="GetDependencyTargetPaths">
  <ItemGroup>
    <TargetPathWithTargetPlatformMoniker Include="$(PkgValidly_SourceGeneration)\lib\netstandard2.0\Validly.SourceGeneration.dll"
                                         IncludeRuntimeDependency="false" />
  </ItemGroup>
</Target>
```

Keep the versions of `Validly.SourceGeneration` and the `Validly` runtime referenced by the consuming project aligned
(the same major version).
