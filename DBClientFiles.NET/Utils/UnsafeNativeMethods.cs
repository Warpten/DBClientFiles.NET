using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DBClientFiles.NET.Utils
{
    [SuppressUnmanagedCodeSecurity]
    internal unsafe class UnsafeNativeMethods
    {
        private UnsafeNativeMethods() { }

        /// <summary>
        /// Allow copying memory from one IntPtr to another. Required as the <see cref="System.Runtime.InteropServices.Marshal.Copy(System.IntPtr, System.IntPtr[], int, int)"/> implementation does not provide an appropriate override.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="count"></param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        [SecurityCritical]
        internal static extern void CopyMemoryPtr(void* dest, void* src, uint count);

        /// <summary>
        ///     Calls the native "memcpy" function.
        /// </summary>
        // Note: SuppressUnmanagedCodeSecurity speeds things up drastically since there is no stack-walk required before moving to native code.
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        internal static extern IntPtr MoveMemory(byte* dest, byte* src, int count);
    }
}
