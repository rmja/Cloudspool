using System;

namespace PrintSpooler.Printing
{
    class PrinterException : Exception
    {
        public PrinterException(string message)
            : base(message)
        {
        }
    }
}
