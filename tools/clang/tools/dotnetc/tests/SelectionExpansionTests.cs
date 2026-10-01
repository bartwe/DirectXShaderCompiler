using System;
using System.Reflection;
using System.Windows.Forms;

class SelectionExpansionTests
{
    static Type resultType = typeof(MainNs.EditorForm).GetNestedType(
        "SelectionExpandResult", BindingFlags.NonPublic);
    static MethodInfo expand = resultType.GetMethod(
        "Expand", BindingFlags.Static | BindingFlags.NonPublic);
    static int cases;
    static int failures;

    [STAThread]
    static int Main()
    {
        for (int caret = 0; caret < 3; ++caret)
            Check("abc", caret, 0, "abc", 0);
        Check("abc", 0, 3, "abc", 0);
        Check("abc", 1, 2, "abc", 0);
        Check("abc", 2, 1, "abc", 0);
        Check("ab", 0, 0, "ab", 0);
        Check("ab", 1, 0, "ab", 0);
        Check("one a1B9", 4, 0, "a1B9", 4);
        Check("one a1B9", 7, 0, "a1B9", 4);
        Check("abc ", 0, 0, "abc", 0);
        Check("abc;", 2, 0, "abc", 0);
        Check("abc def", 2, 0, "abc", 0);
        Check("abc def", 6, 0, "def", 4);
        Check(" %12", 3, 0, "%12", 1);
        Check("@abc", 3, 0, "@abc", 0);
        Check("$abc", 3, 0, "$abc", 0);
        Check("é2", 0, 0, "é2", 0);
        Check("é2", 1, 0, "é2", 0);

        // Preserve the existing empty result for non-token positions and single letters.
        Check("", 0, 0, null, 0);
        Check("abc", 3, 0, null, 0);
        Check("abc;", 3, 0, null, 0);
        Check("abc;", 4, 0, null, 0);
        Check(" abc", 0, 0, null, 0);
        Check("%12", 0, 0, null, 0);
        Check("x", 0, 0, null, 0);
        Check(" x ", 1, 0, null, 0);

        Console.WriteLine("Selection expansion: {0} cases, {1} failures", cases, failures);
        return failures == 0 ? 0 : 1;
    }

    static void Check(string text, int caret, int length, string expectedToken, int expectedStart)
    {
        using (var rtb = new RichTextBox())
        {
            rtb.Text = text;
            rtb.Select(caret, length);
            object result = expand.Invoke(null, new object[] { rtb });
            bool isEmpty = (bool)resultType.GetProperty("IsEmpty").GetValue(result, null);
            string token = (string)resultType.GetProperty("Token").GetValue(result, null);
            int start = (int)resultType.GetProperty("SelectionStart").GetValue(result, null);
            int end = (int)resultType.GetProperty("SelectionEnd").GetValue(result, null);
            string resultText = (string)resultType.GetProperty("Text").GetValue(result, null);
            bool matches = expectedToken == null ? isEmpty :
                !isEmpty && token == expectedToken && start == expectedStart &&
                end == expectedStart + expectedToken.Length && resultText == text;
            ++cases;
            if (!matches)
            {
                ++failures;
                Console.Error.WriteLine(
                    $"Text '{text}', selection ({caret}, {length}): expected '{expectedToken}' at {expectedStart}; " +
                    $"got '{token}' at [{start}, {end}), empty={isEmpty}");
            }
        }
    }
}
