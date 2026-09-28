# Catharsis.UnitTests

The unit-test suite for the [Catharsis](../Catharsis/README.md) library.

## Overview

- **Framework:** [MSTest](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-intro) 4.1
- **Target framework:** `net10.0`
- **Execution:** parallelized at the assembly level
  (`[assembly: Parallelize]`)
- **Scope:** ~4,320 tests mirroring the library's public surface — the test
  folder structure follows the source layout (`Collections/`, `Buffers/`,
  `Advanced/`, `Linq/`, `ComponentModel/`, `DataAnnotations/`, `Concurrency/`,
  `Time/`, `Security/`, `Events/`, `Configuration/`, `Serialization/`, `IO/`,
  `Networking/`, `Text/`, `Numerics/`, `Xml/`, …), so a source file's tests
  live at the matching path.

The test project references `Catharsis` directly and has access to its `internal`
members via `[InternalsVisibleTo("Catharsis.UnitTests")]`, so both public and
internal behavior can be verified.

## Running the tests

Run the whole suite:

```bash
dotnet test Catharsis.UnitTests/Catharsis.UnitTests.csproj
```

Or run everything from the solution root:

```bash
dotnet test Catharsis.slnx
```

Filter to a subset by fully-qualified name or namespace:

```bash
dotnet test --filter "FullyQualifiedName~Catharsis.UnitTests.Collections"
```

Requires the **.NET 10 SDK**.

## Conventions

- One test class per type under test, named `<TypeName>Tests`, placed in the
  namespace/folder that mirrors the source.
- Tests use the standard MSTest attributes (`[TestClass]`, `[TestMethod]`,
  `[DataRow]`) and assertions.
- Because the suite runs in parallel, tests must be independent and avoid shared
  mutable static state.
- New library behavior should arrive with matching tests; keep the build
  warning-clean (analyzers run in-build).
