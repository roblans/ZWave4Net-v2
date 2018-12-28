using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IVersion
    {
        Task<VersionReport> Get(CancellationToken cancellationToken = default(CancellationToken));
        Task<VersionCommandClassReport> CommandClassGet(CommandClass commandClass, CancellationToken cancellationToken = default(CancellationToken));
    }
}