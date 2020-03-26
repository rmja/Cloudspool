using System.Threading.Tasks;

namespace Dispatcher
{
    public interface ISpoolerClient
    {
        public Task RequestInstalledPrinters();
        public Task SpoolJob(SpoolerJob job);
    }
}
