using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IAssociation
    {
        Task<AssociationReport> Get(byte groupID);
        Task<AssociationGroupingsReport> GroupingsGet();
        Task Remove(byte groupID, params byte[] nodes);
        Task Set(byte groupID, params byte[] nodes);
    }
}