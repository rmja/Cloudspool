using System.Drawing.Printing;
using System.Linq;

namespace PrintSpooler.Windows.Printing
{
    static class Printers
    {
        public static string[] GetInstalledPrinters() => PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
    }
}
