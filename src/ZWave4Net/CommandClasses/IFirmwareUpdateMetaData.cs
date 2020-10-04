using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    public interface IFirmwareUpdateMetaData
    {
        Task<FirmwareUpdateMetaDataReport> Get(CancellationToken cancellation = default(CancellationToken));
    }
}