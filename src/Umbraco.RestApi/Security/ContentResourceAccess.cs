using Umbraco.Core.Models;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// A resource object that is passed to the <see cref="ContentPermissionHandler"/>
    /// </summary>
    public class ContentResourceAccess
    {
        public int NodeId { get; }

        /// <summary>
        /// Used to check fo access to virtual content such as root or recycle bin
        /// </summary>
        /// <param name="nodeId"></param>
        public ContentResourceAccess(int nodeId)
        {
            NodeId = nodeId;
        }        
    }
}