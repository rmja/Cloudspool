namespace Dispatcher
{
    public class SpoolerJob
    {
        public string PrinterName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
