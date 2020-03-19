using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintSpooler.Printing.Ghostscript
{
    static class GsWrapper
    {
        public static readonly int ARG_ENCODING_LOCAL = 0;
        public static readonly int ARG_ENCODING_UTF8 = 1;
        public static readonly int ARG_ENCODING_UTF16LE = 2;

        public static int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle)
        {
            if (Environment.Is64BitProcess)
            {
                return Gs64Wrapper.gsapi_new_instance(out pinstance, caller_handle);
            }
            else
            {
                return Gs32Wrapper.gsapi_new_instance(out pinstance, caller_handle);
            }
        }

        public static int gsapi_init_with_args(IntPtr instance, int argc, string[] argv)
        {
            if (Environment.Is64BitProcess)
            {
                return Gs64Wrapper.gsapi_init_with_args(instance, argc, argv);
            }
            else
            {
                return Gs32Wrapper.gsapi_init_with_args(instance, argc, argv);
            }
        }

        public static int gsapi_exit(IntPtr instance)
        {
            if (Environment.Is64BitProcess)
            {
                return Gs64Wrapper.gsapi_exit(instance);
            }
            else
            {
                return Gs32Wrapper.gsapi_exit(instance);
            }
        }

        public static void gsapi_delete_instance(IntPtr instance)
        {
            if (Environment.Is64BitProcess)
            {
                Gs64Wrapper.gsapi_delete_instance(instance);
            }
            else
            {
                Gs32Wrapper.gsapi_delete_instance(instance);
            }
        }

        public static int gsapi_set_arg_encoding(IntPtr instance, int encoding)
        {
            if (Environment.Is64BitProcess)
            {
                return Gs64Wrapper.gsapi_set_arg_encoding(instance, encoding);
            }
            else
            {
                return Gs32Wrapper.gsapi_set_arg_encoding(instance, encoding);
            }
        }
    }
}
