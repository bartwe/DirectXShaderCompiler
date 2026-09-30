// Copyright (C) Microsoft Corporation. All rights reserved.
// This file is distributed under the University of Illinois Open Source License.
// See LICENSE.TXT for details.

#nullable enable

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MainNs;

internal static class HlslHostTests
{
    private static int Main()
    {
        Action[] tests = {
            ShutdownSuccessfulHost,
            ShutdownInactiveHost,
            ShutdownDisconnectedHost,
            ShutdownUnexpectedFailure,
        };
        int failures = 0;
        foreach (Action test in tests)
        {
            try
            {
                test();
                Console.WriteLine("PASS " + test.Method.Name);
            }
            catch (Exception error)
            {
                ++failures;
                Console.Error.WriteLine("FAIL " + test.Method.Name + ": " + error);
            }
        }
        return failures == 0 ? 0 : 1;
    }

    private static HlslHost CreateHost(ShutdownStream stream)
    {
        var host = new HlslHost();
        FieldInfo field = typeof(HlslHost).GetField("host", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Missing host field.");
        field.SetValue(host, stream);
        Assert(host.IsActive, "Injected host must initially be active.");
        return host;
    }

    private static void ShutdownSuccessfulHost()
    {
        var stream = new ShutdownStream();
        HlslHost host = CreateHost(stream);
        host.IsActive = false;
        Assert(stream.WriteCount == 1, "Shutdown must write one message.");
        Assert(!host.IsActive, "Successful shutdown must clear the active host.");
        host.IsActive = false;
        Assert(stream.WriteCount == 1, "Repeated shutdown must not write again.");
    }

    private static void ShutdownInactiveHost()
    {
        var host = new HlslHost();
        host.IsActive = false;
        host.IsActive = false;
        Assert(!host.IsActive, "Inactive shutdown must remain inactive.");
    }

    private static void ShutdownDisconnectedHost()
    {
        var stream = new ShutdownStream { WriteError = new COMException("Host disconnected.") };
        HlslHost host = CreateHost(stream);
        host.IsActive = false;
        Assert(stream.WriteCount == 1, "Shutdown must attempt the write.");
        Assert(!host.IsActive, "Disconnected host must be cleared.");
        host.IsActive = false;
        Assert(stream.WriteCount == 1, "Disconnected shutdown must not retry.");
    }

    private static void ShutdownUnexpectedFailure()
    {
        var error = new InvalidOperationException("Unexpected write failure.");
        var stream = new ShutdownStream { WriteError = error };
        HlslHost host = CreateHost(stream);
        try
        {
            host.IsActive = false;
        }
        catch (InvalidOperationException caught)
        {
            Assert(ReferenceEquals(caught, error), "Unexpected errors must propagate unchanged.");
            Assert(stream.WriteCount == 1, "Shutdown must attempt the write.");
            Assert(host.IsActive, "Unexpected failure must preserve the existing host state.");
            return;
        }
        throw new InvalidOperationException("Unexpected write failure was swallowed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private sealed class ShutdownStream : IStream
    {
        internal int WriteCount;
        internal Exception? WriteError;

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            ++WriteCount;
            byte[] expected = { 8, 0, 0, 0, 2, 0, 0, 0 };
            Assert(cb == expected.Length && pv.Length == expected.Length, "Invalid shutdown message size.");
            for (int i = 0; i < expected.Length; ++i)
                Assert(pv[i] == expected[i], "Invalid shutdown message bytes.");
            Assert(pcbWritten == IntPtr.Zero, "Unexpected write-count pointer.");
            if (WriteError != null)
                throw WriteError;
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead) => throw new NotSupportedException();
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition) => throw new NotSupportedException();
        public void SetSize(long libNewSize) => throw new NotSupportedException();
        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten) => throw new NotSupportedException();
        public void Commit(int grfCommitFlags) => throw new NotSupportedException();
        public void Revert() => throw new NotSupportedException();
        public void LockRegion(long libOffset, long cb, int dwLockType) => throw new NotSupportedException();
        public void UnlockRegion(long libOffset, long cb, int dwLockType) => throw new NotSupportedException();
        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag) => throw new NotSupportedException();
        public void Clone(out IStream ppstm) => throw new NotSupportedException();
    }
}
