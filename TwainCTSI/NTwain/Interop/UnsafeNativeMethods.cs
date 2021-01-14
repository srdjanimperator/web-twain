﻿using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NTwain.Interop
{
    [SuppressUnmanagedCodeSecurity]
    static class UnsafeNativeMethods
    {
        #region mem stuff for twain 1.x

        [DllImport("kernel32", SetLastError = true, EntryPoint = "GlobalAlloc")]
        internal static extern IntPtr WinGlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32", SetLastError = true, EntryPoint = "GlobalFree")]
        internal static extern IntPtr WinGlobalFree(IntPtr hMem);

        [DllImport("kernel32", SetLastError = true, EntryPoint = "GlobalLock")]
        internal static extern IntPtr WinGlobalLock(IntPtr handle);

        [DllImport("kernel32", SetLastError = true, EntryPoint = "GlobalUnlock")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WinGlobalUnlock(IntPtr handle);

        #endregion
    }
}
