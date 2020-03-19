using PrintSpooler.Printing.Ghostscript;
using System.IO;

namespace PrintSpooler.Printing
{
    class PdfPrinter : IPrinter
    {
        private readonly GhostscriptProcessor _ghostscriptProcessor = new GhostscriptProcessor();

        public string PrinterName { get; set; }

        public PdfPrinter(string printerName)
        {
            PrinterName = printerName;
        }

        public void Print(byte[] content)
        {
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, content);

            Print(tempFilePath);

            File.Delete(tempFilePath);
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

            _ghostscriptProcessor.Process(args);
        }
    }
}
