using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FFSharp
{
    /// <summary>
    /// Provides P/Invoke imports of "kernel32.dll" functions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class NativeMethods
    {
        /// <summary>
        /// In combination with an absolute path, causes <see cref="LoadLibraryEx"/> to use that
        /// path. Without it, the search path will always be inserted.
        /// </summary>
        public const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

        /// <summary>
        /// Load a module.
        /// </summary>
        /// <remarks>
        /// See the MSDN documentation at:
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms684179(v=vs.85).aspx
        /// </remarks>
        /// <param name="APathOrName">The path to or name of the module.</param>
        /// <param name="AHandle">Reserved, must be <see cref="IntPtr.Zero"/>.</param>
        /// <param name="AFlags">Bitflags enum to modify the search behaviour with.</param>
        /// <returns>
        /// The <see cref="IntPtr"/> to the loaded module; or <see cref="IntPtr.Zero"/> on failure.
        /// </returns>
        [
            DllImport(
                "kernel32.dll",
                SetLastError = true,
                CharSet = CharSet.Unicode,
                ThrowOnUnmappableChar = true
            )
        ]
        public static extern IntPtr LoadLibraryEx(string APathOrName, IntPtr AHandle, uint AFlags);

        /// <summary>
        /// Release a module.
        /// </summary>
        /// <remarks>
        /// See the MSDN documentation at:
        /// https://msdn.microsoft.com/de-de/library/windows/desktop/ms683152(v=vs.85).aspx
        /// </remarks>
        /// <param name="AModule">The module to release.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr AModule);
    }
}
