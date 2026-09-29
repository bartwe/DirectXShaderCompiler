"""Run with: python test_arguments.py /path/to/dndxc.exe"""

import pathlib
import subprocess
import sys
import tempfile
import unittest


class ArgumentTests(unittest.TestCase):
    def setUp(self):
        directory = tempfile.TemporaryDirectory()
        self.addCleanup(directory.cleanup)
        self.filename = str(pathlib.Path(directory.name) / "test shader.hlsl")
        self.source = "float4 main() : SV_Target { return 1; }\n"
        pathlib.Path(self.filename).write_text(self.source)

    def run_dndxc(self, args):
        return subprocess.run(
            self.command + args, capture_output=True, text=True, timeout=10
        )

    def test_positional_filename(self):
        for args in (
            [self.filename],
            ["-s", "800", "600", self.filename],
            [self.filename, "-s", "800", "600"],
        ):
            with self.subTest(args=args):
                result = self.run_dndxc(args)
                self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
                self.assertIn(self.filename + ":\n" + self.source, result.stdout)
                self.assertIn("tokens found.", result.stdout)

    def test_invalid_arguments(self):
        for args in (
            ["-s", "800", "600"],
            ["-s"],
            ["-s", "800"],
            ["-s", "bad", "600", self.filename],
            ["-s", "800", "bad", self.filename],
            ["--unknown"],
            [self.filename, "--unknown"],
            [self.filename, self.filename],
        ):
            with self.subTest(args=args):
                result = self.run_dndxc(args)
                self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
                self.assertIn("USAGE:", result.stdout)
                self.assertNotIn("tokens found.", result.stdout)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    ArgumentTests.command = sys.argv[1:]
    unittest.main(argv=sys.argv[:1])
