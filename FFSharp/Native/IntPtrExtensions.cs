using System;
using System.Diagnostics.CodeAnalysis;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides static extension methods for the <see cref="IntPtr"/> type.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class IntPtrExtensions
    {
        /// <summary>
        /// Get the address value of this <see cref="IntPtr"/> as a <see cref="UInt64"/>.
        /// </summary>
        /// <param name="AIntPtr">The <see cref="IntPtr"/>.</param>
        /// <returns>The address value as a <see cref="UInt64"/>.</returns>
        public static ulong ToUInt64(this IntPtr AIntPtr) => unchecked((ulong)AIntPtr.ToInt64());
    }
}
