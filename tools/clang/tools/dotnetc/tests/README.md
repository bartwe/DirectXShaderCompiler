# HlslHost shutdown regression

With the .NET 10 SDK, from this directory:

```sh
dotnet build HlslHostTests.csproj -c Release -m:4 --artifacts-path /tmp/hlsl-host-tests
dotnet /tmp/hlsl-host-tests/bin/HlslHostTests/release/HlslHostTests.dll
```

The tests compile the actual `HlslHost.cs` and inject an `IStream` stub into its
private host field. They check shutdown message bytes, inactive state, repeated
shutdown, COM disconnection, and propagation of unexpected exceptions. They run
without Windows COM registration or a GPU; COM activation and rendering still
require validation on Windows.
