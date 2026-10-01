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
