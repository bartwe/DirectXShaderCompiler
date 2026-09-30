# AsmColorizer regression tests

Run the standalone colorizer tests with the .NET 10 SDK from this directory:

```sh
dotnet run --project AsmColorizerTests.csproj --artifacts-path /path/to/test-artifacts -c Release
```

The tests compile the production colorizer directly and require no Windows UI
or native shader compiler. They check exact token ranges and classifications
while fully enumerating both `GetColorRanges` overloads.
