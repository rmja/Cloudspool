namespace Intercom.Models
{
    public class PrintJob
    {
        public string PrinterName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
