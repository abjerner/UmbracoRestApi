using System.Linq;
using Microsoft.Owin.Security.Authorization;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// An AuthZ handler to check if the user has access to the specified section
    /// </summary>
    public class UmbracoSectionAccessHandler : AuthorizationHandler<UmbracoSectionAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UmbracoSectionAccessRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == Core.Constants.Security.AllowedApplicationsClaimType && c.Issuer == UmbracoBackOfficeIdentity.Issuer))
            {
                context.Fail();
                return Task.FromResult(0);
            }

            var allowedApps = context.User.FindAll(x => x.Type == Core.Constants.Security.AllowedApplicationsClaimType).Select(app => app.Value).ToList();

            var allowed = allowedApps.Contains(requirement.Section);

            if (allowed)
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.FromResult(0);
        }
    }
}