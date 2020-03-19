using System;
using System.Runtime.InteropServices;

namespace PrintSpooler.Printing.Ghostscript
{
    class GhostscriptProcessor
    {
        private static object _gsLock = new object();

        public void Process(string[] args)
        {
            IntPtr instanceHandle = IntPtr.Zero;
            Exception exception = null;

            lock (_gsLock)
            {
                int status = GsWrapper.gsapi_new_instance(out instanceHandle, IntPtr.Zero);

                if (status >= 0)
                {
                    status = GsWrapper.gsapi_set_arg_encoding(instanceHandle, GsWrapper.ARG_ENCODING_UTF16LE);

                    if (status >= 0)
                    {
                        status = GsWrapper.gsapi_init_with_args(instanceHandle, args.Length, args);

                        if (status >= 0)
                        {
                            GsWrapper.gsapi_exit(instanceHandle);
                        }
                        else
                        {
                            exception = new ExternalException("Failed to initialize Ghostscript with arguments.", status);
                        }
                    }
                    else
                    {
                        exception = new ExternalException("Failed to set Ghostscript argument encoding.", status);
                    }

                    GsWrapper.gsapi_delete_instance(instanceHandle);
                }
                else
                {
                    exception = new ExternalException("Failed to create Ghostscript instance.", status);
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }
    }
}
