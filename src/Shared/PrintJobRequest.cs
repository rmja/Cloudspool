namespace Intercom
{
    public class PrintJobRequest
    {
        public int SpoolerId { get; set; }
        public string PrinterName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
