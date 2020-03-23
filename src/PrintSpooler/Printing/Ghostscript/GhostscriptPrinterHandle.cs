using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PrintSpooler.Printing.Ghostscript
{
    class GhostscriptPrinterHandle : IPrinterHandle
    {
        public string PrinterName { get; set; }

        public GhostscriptPrinterHandle(string printerName)
        {
            PrinterName = printerName;
        }

        public void Print(byte[] content)
        {
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, content);

            try
            {
                Print(tempFilePath);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        public void Print(string filePath)
        {
            var args = new string[] {
                "-q",
                "-dBATCH",
                "-dNOPAUSE",
                "-dNOPROMPT",
                "-dQUIET",
                "-dSAFER",
                "-dNoCancel",
                "-sDEVICE=mswinpr2",
                "-sOutputFile=%printer%" + PrinterName,
                filePath
            };

            Process(args);
        }

        public void Process(string[] args)
        {
            Exception exception = null;

            int status = GsWrapper.gsapi_new_instance(out var instanceHandle, IntPtr.Zero);

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

            if (exception != null)
            {
                throw exception;
            }
        }
    }
}
