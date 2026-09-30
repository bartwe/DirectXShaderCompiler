// Copyright (C) Microsoft Corporation. All rights reserved.
// This file is distributed under the University of Illinois Open Source License.
// See LICENSE.TXT for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MainNs
{
    static class AsmColorizerTests
    {
        private static void AssertRanges(string text, IEnumerable<AsmRange> actual, AsmRange[] expected)
        {
            AsmRange[] ranges = actual.ToArray();
            if (ranges.Length != expected.Length)
                throw new Exception($"'{text}': expected {expected.Length} ranges, got {ranges.Length}.");

            for (int i = 0; i < ranges.Length; ++i)
            {
                if (ranges[i].Start != expected[i].Start || ranges[i].Length != expected[i].Length ||
                    ranges[i].RangeKind != expected[i].RangeKind)
                    throw new Exception($"'{text}': range {i} was ({ranges[i].Start}, {ranges[i].Length}, {ranges[i].RangeKind}), " +
                        $"expected ({expected[i].Start}, {expected[i].Length}, {expected[i].RangeKind}).");
            }
        }

        private static void CheckRanges(string text, AsmRange[] expected)
        {
            var colorizer = new AsmColorizer();
            AssertRanges(text, colorizer.GetColorRanges(text), expected);
            AssertRanges(text, colorizer.GetColorRanges(text, 0, text.Length), expected);
        }

        private static void Main()
        {
            CheckRanges("i", new[] { new AsmRange(0, 1, AsmRangeKind.Other) });
            CheckRanges("foo i", new[] {
                new AsmRange(0, 3, AsmRangeKind.Other),
                new AsmRange(3, 1, AsmRangeKind.WS),
                new AsmRange(4, 1, AsmRangeKind.Other)
            });
            AssertRanges("foo i", new AsmColorizer().GetColorRanges("foo i", 4, 5),
                new[] { new AsmRange(4, 1, AsmRangeKind.Other) });

            foreach (string separator in new[] { " ", "\t", "\n", "," })
            {
                AsmRangeKind separatorKind = separator == "," ? AsmRangeKind.Punctuation : AsmRangeKind.WS;
                CheckRanges("i" + separator + "i", new[] {
                    new AsmRange(0, 1, AsmRangeKind.Other),
                    new AsmRange(1, 1, separatorKind),
                    new AsmRange(2, 1, AsmRangeKind.Other)
                });
                CheckRanges("i" + separator, new[] {
                    new AsmRange(0, 1, AsmRangeKind.Other),
                    new AsmRange(1, 1, separatorKind)
                });
            }

            foreach (string token in new[] { "i1", "i8", "i32", "i64" })
                CheckRanges(token, new[] { new AsmRange(0, token.Length, AsmRangeKind.LLVMTypeName) });
            foreach (string token in new[] { "I", "I32", "index" })
                CheckRanges(token, new[] { new AsmRange(0, token.Length, AsmRangeKind.Other) });

            CheckRanges("i:", new[] { new AsmRange(0, 2, AsmRangeKind.Label) });
            CheckRanges("icmp", new[] { new AsmRange(0, 4, AsmRangeKind.Instruction) });
            CheckRanges("internal", new[] { new AsmRange(0, 8, AsmRangeKind.Keyword) });
            CheckRanges("", Array.Empty<AsmRange>());
            Console.WriteLine("AsmColorizer regression tests passed (43 range enumerations).");
        }
    }
}
