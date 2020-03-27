using System;

namespace PrintSpooler.Printing
{
    interface IPrinterHandle
    {
        void Print(byte[] content);
    }
}
