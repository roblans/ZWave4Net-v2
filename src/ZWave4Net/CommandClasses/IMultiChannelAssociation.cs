using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    /// <summary>
    /// The Multi Channel Association Command Class is used to manage associations to Multi Channel End Point destinations as well as to NodeID destination
    /// </summary>
    public interface IMultiChannelAssociation
    {
        /// <summary>
        /// This command is used to request the current destinations of a given association group. 
        /// </summary>
        /// <param name="groupID">This field is used to specify the actual association group. Grouping Identifiers MUST be assigned in a consecutive range starting from 1.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<MultiChannelAssociationReport> Get(byte groupId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// This command is used to advertise the maximum number of association groups implemented by this node.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<AssociationGroupingsReport> GetGroupings(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// This command is used to request that one or more destinations are added to a given association group
        /// </summary>
        /// <param name="groupID">This field is used to specify the actual association group. Grouping Identifiers MUST be assigned in a consecutive range starting from 1.</param>
        /// <param name="nodes">This field specifies a list of NodeID destinations that are to be added to the specified association group as a NodeID association</param>
        /// <param name="endpoints">This fields specify a list of Endpoints which are to be added to the specified association group as an End Point association</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task Set(byte groupID, byte[] nodes, EndpointAssociation[] endpoints, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// This command is used to remove Nodes and Endpoint destinations from a given association group
        /// </summary>
        /// <param name="groupID">This field is used to specify the actual association group. Grouping Identifiers MUST be assigned in a consecutive range starting from 1.</param>
        /// <param name="nodes">This field specifies a list of NodeID destinations that are to be removed from the specified association group as a NodeID association</param>
        /// <param name="endpoints">This fields specify a list of Endpoints which are to be removed from the specified association group as an End Point association</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task Remove(byte groupID, byte[] nodes, EndpointAssociation[] endpoints, CancellationToken cancellationToken = default(CancellationToken));
    }
}