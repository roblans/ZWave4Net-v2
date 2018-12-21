using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IAssociation
    {
        Task<AssociationReport> Get(byte groupID, CancellationToken cancellation = default(CancellationToken));
        Task<AssociationGroupingsReport> GetGroupings(CancellationToken cancellation = default(CancellationToken));
        Task Remove(byte groupID, byte[] nodes, CancellationToken cancellation = default(CancellationToken));
        Task Set(byte groupID, byte[] nodes, CancellationToken cancellation = default(CancellationToken));
    }
}