using System;

namespace Cloudspool.PrintSpooler.Printing
{
    interface IPrinterHandle
    {
        void Print(byte[] content);
    }
}
