using System;
using System.Runtime.InteropServices;

namespace PrintSpooler.Printing
{
    class RawPrinter : IPrinter
    {
        public string PrinterName { get; set; }

        public RawPrinter(string printerName)
        {
            PrinterName = printerName;
        }

        public void Print(byte[] content)
        {
            IntPtr printerHandle = IntPtr.Zero;
            WinSpoolWrapper.DOCINFOA di = new WinSpoolWrapper.DOCINFOA();
            Exception exception = null;

            di.pDocName = "Team Booking Print Spooler Job";
            di.pDataType = "RAW";

            if (WinSpoolWrapper.OpenPrinter(PrinterName.Normalize(), out printerHandle, IntPtr.Zero))
            {
                if (WinSpoolWrapper.StartDocPrinter(printerHandle, 1, di))
                {
                    if (WinSpoolWrapper.StartPagePrinter(printerHandle))
                    {
                        int written;
                        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(content.Length);

                        Marshal.Copy(content, 0, pUnmanagedBytes, content.Length);

                        if (!WinSpoolWrapper.WritePrinter(printerHandle, pUnmanagedBytes, content.Length, out written))
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

            if (exception != null)
            {
                throw exception;
            }
        }
    }
}
