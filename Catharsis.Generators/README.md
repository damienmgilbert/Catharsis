# Catharsis.Generators

Roslyn [incremental source generators](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
that ship separately from the [Catharsis](../Catharsis/README.md) library. The
library references it as a build-time analyzer only (see `OrderStatus` and`OrderDraftViewModel` in `Catharsis.Domain`); any consumer can add it the same wayand and gets the generators' attributes emitted straight into their own
compilation, so no runtime dependency is introduced.

The project targets `netstandard2.0` (as analyzers must) and depends on
`Microsoft.CodeAnalysis.CSharp`, marked `PrivateAssets="all"` so it does not
flow to consumers.

## Using it

```xml
<ProjectReference Include="..\Catharsis.Generators\Catharsis.Generators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

## Generators

### `[AutoNotify]`

Put it on a `partial`, top-level, non-generic class. Every instance field gets a
public property that raises `PropertyChanged` only when the value actually
changes, and the class is made to implement `INotifyPropertyChanged`.

```csharp
using Catharsis.Generators;

[AutoNotify]
public partial class Person
{
    private string _name = "";   // -> public string Name { get; set; }
    private int age;             // -> public int Age { get; set; }
}
```

A field `_name` or `name` becomes `Name`. Unsealed classes get a
`protected virtual void OnPropertyChanged(string)` that subclasses can override;
sealed classes get a private one.

| Id | Severity | Meaning |
| --- | --- | --- |
| `CATGEN001` | Error | The class is not declared `partial`. |
| `CATGEN002` | Error | The class is nested or generic. |
| `CATGEN003` | Warning | A field's property name already exists (or equals the field name), so it was skipped. |

### `[EnumExtensions]`

Put it on a top-level enum. A static `{Name}Extensions` class is generated with
`switch`-based `ToStringFast`, `TryParseFast` (exact, case-sensitive names) and
`IsDefinedFast`, avoiding the reflection that `Enum.ToString` and
`Enum.TryParse` perform. When several names share a value, `ToStringFast`
returns the first declared name.

| Id | Severity | Meaning |
| --- | --- | --- |
| `CATGEN004` | Error | The enum is nested inside another type. |

## Tests

`Catharsis.Generators.UnitTests` runs each generator in a real Roslyn
compilation, asserts on the diagnostics and generated text, confirms the result
compiles, and loads the compiled assembly to call the generated code.

## License & authorship

Authored by **Damien M Gilbert**.
