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

        const string DllName = "gs";

        static GsWrapper()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(directoryPath, GetNativeLibraryRelativePath());
            
            NativeLibrary.Load(path);
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
    }
}
