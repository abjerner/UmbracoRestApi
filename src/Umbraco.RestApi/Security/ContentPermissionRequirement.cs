using Microsoft.Owin.Security.Authorization;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// An AuthZ requirement for validating that the user has a specific permission for a content item
    /// </summary>
    public class ContentPermissionRequirement : IAuthorizationRequirement
    {
        public string[] Permissions { get; }

        public ContentPermissionRequirement(params string[] permissions)
        {
            Permissions = permissions;
        }
    }
}