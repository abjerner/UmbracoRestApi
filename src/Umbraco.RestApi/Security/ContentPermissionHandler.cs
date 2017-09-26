using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Authorization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// An AuthZ handler to check if the user has access to the specified section
    /// </summary>
    public class ContentPermissionHandler : AuthorizationHandler<ContentPermissionRequirement, ContentResourceAccess>
    {
        private readonly ServiceContext _services;

        public ContentPermissionHandler(ServiceContext services)
        {
            _services = services;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ContentPermissionRequirement requirement, ContentResourceAccess resource)
        {
            var user = context.User.GetUserFromClaims(_services.UserService);
            if (user == null)
            {
                context.Fail();
                return Task.FromResult(0);
            }

            IContent content = null;
            if (resource.NodeId != Constants.System.Root && resource.NodeId != Constants.System.RecycleBinContent)
            {
                content = _services.ContentService.GetById(resource.NodeId);
                if (content == null)
                {
                    context.Fail();
                    return Task.FromResult(0);
                }
            }

            var allowed = CheckPermissions(user, resource.NodeId, new[]
            {
                //currently permissions are a single letter
                requirement.Permission[0]
            }, content);

            if (allowed)
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.FromResult(0);
        }
        
        private bool CheckPermissions(IUser user, int nodeId, char[] permissionsToCheck, IContent contentItem)
        {
            var tempStorage = new Dictionary<string, object>();
            //TODO: Using reflection, this will be public in 7.7.2
            var result = (bool)typeof(ContentController).CallStaticMethod("CheckPermissions",
                tempStorage,
                user,
                _services.UserService, _services.ContentService, _services.EntityService,
                nodeId, permissionsToCheck, contentItem);
            return result;
        }
    }
}