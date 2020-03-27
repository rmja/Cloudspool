using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PrintSpooler.Printing.Ghostscript
{
    static class GsWrapper
    {
        public static readonly int ARG_ENCODING_LOCAL = 0;
        public static readonly int ARG_ENCODING_UTF8 = 1;
        public static readonly int ARG_ENCODING_UTF16LE = 2;
        private static readonly string _libPath;
        private static IntPtr _libHandle;
        const string DllName = "gs";

        [StructLayout(LayoutKind.Sequential)]
        public struct GSVersion
        {
            public IntPtr product;
            public IntPtr copyright;
            public int revision;
            public int revisionDate;
        }

        static GsWrapper()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _libPath = Path.Combine(directoryPath, GetNativeLibraryRelativePath());
            
            _libHandle = NativeLibrary.Load(_libPath);
        }

        public static void ReloadLibrary()
        {
            NativeLibrary.Free(_libHandle);
            _libHandle = NativeLibrary.Load(_libPath);
        }

        static string GetNativeLibraryRelativePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                return Path.Combine("Native", "win32", "gs.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return Path.Combine("Native", "win64", "gs.dll");
            }

            throw new NotSupportedException("Unsupported platform");
        }

        [DllImport(DllName)]
        public static extern int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);

        [DllImport(DllName)]
        public static extern int gsapi_init_with_args(IntPtr instance, int argc, string[] argv);

        [DllImport(DllName)]
        public static extern int gsapi_exit(IntPtr instance);

        [DllImport(DllName)]
        public static extern void gsapi_delete_instance(IntPtr instance);

        [DllImport(DllName)]
        public static extern int gsapi_set_arg_encoding(IntPtr instance, int encoding);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        public static extern int gsapi_revision(ref GSVersion version, int len);

        public static void gsapi_revision(ref GSVersion version) => gsapi_revision(ref version, Marshal.SizeOf(version));
    }
}
