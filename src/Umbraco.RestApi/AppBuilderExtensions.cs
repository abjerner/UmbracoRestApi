using System;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Authorization;
using Microsoft.Owin.Security.Authorization.Infrastructure;
using Owin;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.RestApi.Security;
using Umbraco.Web.Security.Identity;

namespace Umbraco.RestApi
{
    public static class AppBuilderExtensions
    {
        public static void ConfigureUmbracoRestApi(this IAppBuilder app, UmbracoRestApiOptions options, ApplicationContext applicationContext)
        {
            if (options == null) throw new ArgumentNullException("options");
            UmbracoRestApiOptionsInstance.Options = options;

            app.ConfigureUmbracoRestApiAuthorizationPolicies(applicationContext);
        }

        /// <summary>
        /// Authorization for the rest API uses authorization schemes, see https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationContext"></param>
        /// <remarks>
        /// This is a .NET Core approach to authorization but we have a package that has backported all of this for us for .NET 452
        /// </remarks>
        public static void ConfigureUmbracoRestApiAuthorizationPolicies(this IAppBuilder app, ApplicationContext applicationContext)
        {
            app.UseAuthorization(options =>
            {
                options.AddPolicy(
                    AuthorizationPolicies.PublishedContentRead,
                    policy => policy.RequireSessionIdOrRestApiClaim());
                
                options.AddPolicy(
                    AuthorizationPolicies.ContentRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionBrowse.Instance.Letter.ToString()));
                    });
                
                options.AddPolicy(
                    AuthorizationPolicies.MediaRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Media));
                    });

                options.AddPolicy(
                    AuthorizationPolicies.MemberRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Members));
                    });

                var handlers = new IAuthorizationHandler[]
                {
                    new UmbracoSectionAccessHandler(), 
                    new ContentPermissionHandler(applicationContext.Services)
                };
                options.Dependencies.Service = new DefaultAuthorizationService(new DefaultAuthorizationPolicyProvider(options), handlers);

                app.CreatePerOwinContext(() => new AuthorizationServiceWrapper(options.Dependencies.Service));
            });
        }

        /// <summary>
        /// Used to enable back office cookie authentication for the REST API calls
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoCookieAuthenticationForRestApi(this IAppBuilder app, ApplicationContext appContext)
        {
            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return app;

            var authOptions = new UmbracoBackOfficeCookieAuthOptions(
                UmbracoConfig.For.UmbracoSettings().Security,
                GlobalSettings.TimeOutInMinutes,
                GlobalSettings.UseSSL)
            {
                Provider = new BackOfficeCookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user 
                    // logs in. This is a security feature which is used when you 
                    // change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                            TimeSpan.FromMinutes(30),
                            (manager, user) => user.GenerateUserIdentityAsync(manager),
                            identity => identity.GetUserId<int>()),
                }
            };

            //This is what will ensure that the rest api calls are auth'd
            authOptions.CookieManager = new RestApiCookieManager();

            app.UseCookieAuthentication(authOptions);

            return app;
        }
        
    }
}
