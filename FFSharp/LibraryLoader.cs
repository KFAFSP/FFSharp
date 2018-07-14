using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Provides a module loading mechanism for the FFmpeg library wrapper.
    /// </summary>
    internal static class LibraryLoader
    {
        /// <summary>
        /// Array constant of all library names we absolutely require.
        /// </summary>
        [NotNull]
        static readonly Tuple<string, int>[] _FRequiredLibs =
        {
            Tuple.Create("avcodec", 58),
            Tuple.Create("avdevice", 58),
            Tuple.Create("avformat", 58),
            Tuple.Create("avutil", 56)
        };

        /// <summary>
        /// Root path of the library folder.
        /// </summary>
        static string _FRootPath;

        /// <summary>
        /// Caches handles of loaded libraries.
        /// </summary>
        static readonly Dictionary<string, IntPtr> _FLibs =
            new Dictionary<string, IntPtr>();

        /// <summary>
        /// Initialize the FFmpeg library loader.
        /// </summary>
        /// <param name="ARootPath">The root path to the DLL files.</param>
        public static void Init([NotNull] string ARootPath)
        {
            Debug.Assert(ARootPath != null, "Root path is null.");

            // Ensure that we have an absolute path.
            var fullPath = Path.GetFullPath(ARootPath);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException();
            }

            // Set the root path.
            _FRootPath = fullPath;

            // Replace the default platform-independent loader function with one that actually
            // works on Win64. (For some reason, Set/AddDllDirectory and default searching only
            // works on .NET Core projects ?!). It was improved in recent (4.x) versions, but
            // let's stick to things that we know work.
            // Also, must compile with x64 since these binaries are x64 aswell.
            Unsafe.ffmpeg.GetOrLoadLibrary = GetOrLoadLibrary;

            // We always need these, so load them right now.
            foreach (var lib in _FRequiredLibs)
            {
                GetOrLoadLibrary(lib.Item1, lib.Item2);
            }
        }

        /// <summary>
        /// Get or load a cached FFmpeg library.
        /// </summary>
        /// <param name="AName">The name of the library.</param>
        /// <param name="AVersion">The required version.</param>
        /// <returns><see cref="IntPtr"/> handle of the loaded library.</returns>
        static IntPtr GetOrLoadLibrary([NotNull] string AName, int AVersion)
        {
            Debug.Assert(
                AName != null,
                "Name is null.",
                "This indicates a severe logic error in the code."
            );

            // Build library name.
            var libName = $"{AName}-{AVersion}";

            // Do a cache lookup.
            if (_FLibs.TryGetValue(libName, out var handle))
                return handle;

            // Cache miss: try to load the library.
            var libPath = Path.Combine(_FRootPath, libName + ".dll");
            if (!File.Exists(libPath))
            {
                // No such file.
                var nativeError = new FileNotFoundException("No such file.", libPath);
                // Throw an UnrecoverableError that propagates to the top.
                throw new FFmpegException("Missing required library.", nativeError);
            }

            handle = NativeMethods.LoadLibraryEx(
                libPath,
                IntPtr.Zero,
                NativeMethods.LOAD_WITH_ALTERED_SEARCH_PATH);
            if (handle == IntPtr.Zero)
            {
                // Get and lookup the error code.
                var code = Marshal.GetLastWin32Error();
                var nativeError = new Win32Exception(code);

                // Throw an UnrecoverableError that propagates to the top.
                throw new FFmpegException("Unable to load library.", nativeError);
            }

            // Cache and return.
            _FLibs[libName] = handle;
            return handle;
        }

        /// <summary>
        /// Cleanup all acquired FFmpeg modules.
        /// </summary>
        public static void Cleanup()
        {
            // Remove our library loader.
            Unsafe.ffmpeg.GetOrLoadLibrary = null;

            // Free all loaded library handles.
            // Technically not necessary before app exit, since they handles are refcounted.
            var libs = _FLibs.ToList();
            _FLibs.Clear();
            foreach (var lib in libs)
            {
                NativeMethods.FreeLibrary(lib.Value);
            }

            _FLibs.Clear();
        }
    }
}
