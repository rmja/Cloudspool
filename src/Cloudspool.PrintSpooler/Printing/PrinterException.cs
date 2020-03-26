using System;

namespace Cloudspool.PrintSpooler.Printing
{
    class PrinterException : Exception
    {
        public PrinterException(string message)
            : base(message)
        {
        }
    }
}
