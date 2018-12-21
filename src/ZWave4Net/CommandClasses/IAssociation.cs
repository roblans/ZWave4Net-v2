using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    /// <summary>
    /// The Association interface is used to manage associations to NodeID destinations. A NodeID destination may be a simple device or the Root Device of a Multi Channel device.
    /// </summary>
    public interface IAssociation
    {
        /// <summary>
        /// Request the current destinations of a given association group.
        /// </summary>
        /// <param name="groupID">This field is used to specify the actual association group. Grouping Identifiers MUST be assigned in a consecutive range starting from 1.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation</returns>
        Task<AssociationReport> Get(byte groupID, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// This command is used to request the number of association groups that this node supports.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation</returns>
        Task<AssociationGroupingsReport> GetGroupings(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// This command is used to remove destinations from a given association group.
        /// </summary>
        /// <param name="groupID">This field is used to specify from which association group the specified NodeID destinations should be removed.</param>
        /// <param name="nodes">This field is used to specify from which association group the specified NodeID destinations should be removed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation</returns>
        Task Remove(byte groupID, byte[] nodes, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// This command is used to add destinations to a given association group.
        /// </summary>
        /// <param name="groupID">This field is used to specify the actual association group. Grouping Identifiers MUST be assigned in a consecutive range starting from 1.</param>
        /// <param name="nodes">This field specifies a list of NodeIDs that are to be added to the specified association group</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation</returns>
        Task Set(byte groupID, byte[] nodes, CancellationToken cancellationToken = default(CancellationToken));
    }
}