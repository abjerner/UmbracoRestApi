using Umbraco.Core.Models;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// A resource object that is passed to the <see cref="ContentPermissionHandler"/>
    /// </summary>
    public class ContentResourceAccess
    {
        public int[] NodeIds { get; }
        
        public ContentResourceAccess(int[] nodeIds)
        {
            NodeIds = nodeIds;
        }
        
        public ContentResourceAccess(int nodeId)
        {
            NodeIds = new []{nodeId};
        }        
    }
}