using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IFirmwareUpdateMetaData
    {
        Task<FirmwareUpdateMetaDataReport> Get(CancellationToken cancellation = default(CancellationToken));
    }
}