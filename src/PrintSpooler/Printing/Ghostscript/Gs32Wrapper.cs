using System;
using System.Runtime.InteropServices;

namespace PrintSpooler.Printing.Ghostscript
{
    static class Gs32Wrapper
    {
        [DllImport("gsdll32.dll")]
        public static extern int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);

        [DllImport("gsdll32.dll")]
        public static extern int gsapi_init_with_args(IntPtr instance, int argc, string[] argv);

        [DllImport("gsdll32.dll")]
        public static extern int gsapi_exit(IntPtr instance);

        [DllImport("gsdll32.dll")]
        public static extern void gsapi_delete_instance(IntPtr instance);

        [DllImport("gsdll32.dll")]
        public static extern int gsapi_set_arg_encoding(IntPtr instance, int encoding);
    }
}