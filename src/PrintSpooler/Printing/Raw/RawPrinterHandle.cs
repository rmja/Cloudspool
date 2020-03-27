using System;
using System.Runtime.InteropServices;

namespace PrintSpooler.Printing.Raw
{
    class RawPrinterHandle : IPrinterHandle
    {
        public string PrinterName { get; set; }

        public RawPrinterHandle(string printerName)
        {
            PrinterName = printerName;
        }

        public void Print(byte[] content)
        {
            Exception exception = null;
            WinSpoolWrapper.DOCINFOA di = new WinSpoolWrapper.DOCINFOA
            {
                pDocName = "Cloudspool",
                pDataType = "RAW"
            };

            if (WinSpoolWrapper.OpenPrinter(PrinterName.Normalize(), out var printerHandle, IntPtr.Zero))
            {
                if (WinSpoolWrapper.StartDocPrinter(printerHandle, 1, di))
                {
                    if (WinSpoolWrapper.StartPagePrinter(printerHandle))
                    {
                        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(content.Length);

                        Marshal.Copy(content, 0, pUnmanagedBytes, content.Length);

                        if (!WinSpoolWrapper.WritePrinter(printerHandle, pUnmanagedBytes, content.Length, out var written))
                        {
                            exception = new PrinterException("Unable to write to printer.");
                        }

                        Marshal.FreeCoTaskMem(pUnmanagedBytes);

                        WinSpoolWrapper.EndPagePrinter(printerHandle);
                    }
                    else
                    {
                        exception = new PrinterException("Cannot start page.");
                    }

                    WinSpoolWrapper.EndDocPrinter(printerHandle);
                }
                else
                {
                    exception = new PrinterException("Cannot start document.");
                }

                WinSpoolWrapper.ClosePrinter(printerHandle);
            }
            else
            {
                exception = new PrinterException("Printer not found");
            }

            if (exception is object)
            {
                throw exception;
            }
        }
    }
}
