# dndxc regression tests

After building `dndxc.exe`, compile and run the selection expansion tests from a
Developer Command Prompt, placing the test executable beside `dndxc.exe`:

```cmd
csc /checked+ /r:System.Windows.Forms.dll /r:path\to\dndxc.exe /out:path\to\SelectionExpansionTests.exe SelectionExpansionTests.cs
path\to\SelectionExpansionTests.exe
```

The tests invoke the editor's private expansion helper through reflection using
real `RichTextBox` controls. They do not require the native DXC library. On Linux,
use `mcs` in place of `csc` and run with
`xvfb-run -a mono path/to/SelectionExpansionTests.exe`.

Run the argument tests with Python and a build that has the native DXC library:

```cmd
python test_arguments.py path\to\dndxc.exe
```

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

# AsmColorizer regression tests

Run the standalone colorizer tests with the .NET 10 SDK from this directory:

```sh
dotnet run --project AsmColorizerTests.csproj --artifacts-path /path/to/test-artifacts -c Release
```

The tests compile the production colorizer directly and require no Windows UI
or native shader compiler. They check exact token ranges and classifications
while fully enumerating both `GetColorRanges` overloads.
