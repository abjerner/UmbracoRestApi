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
        /// <summary>
        /// Required call to enable the REST API
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationContext"></param>
        /// <param name="options"></param>
        public static void UseUmbracoRestApi(this IAppBuilder app, 
            ApplicationContext applicationContext, 
            UmbracoRestApiOptions options = null)
        {
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));
            UmbracoRestApiOptionsInstance.Options = options ?? new UmbracoRestApiOptions();

            app.UseUmbracoRestApiAuthorizationPolicies(applicationContext, UmbracoRestApiOptionsInstance.Options.CustomAuthorizationPolicyCallback);
        }


        /// <summary>
        /// Authorization for the rest API uses authorization schemes, see https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationContext"></param>
        /// <param name="customPolicyCallback"></param>
        /// <remarks>
        /// This is a .NET Core approach to authorization but we have a package that has backported all of this for us for .NET 452
        /// </remarks>
        internal static void UseUmbracoRestApiAuthorizationPolicies(
            this IAppBuilder app, 
            ApplicationContext applicationContext,
            Func<string, AuthorizationPolicy, Action<AuthorizationPolicyBuilder>> customPolicyCallback = null)
        {
            app.UseAuthorization(options =>
            {
                AddAuthorizationPolicy(options, 
                    AuthorizationPolicies.PublishedContentRead,
                    policy => policy.RequireSessionIdOrRestApiClaim(),
                    customPolicyCallback);

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionBrowse.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentCreate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionNew.Instance.Letter.ToString(), ActionUpdate.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentUpdate,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionUpdate.Instance.Letter.ToString(), ActionPublish.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);
                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.ContentDelete,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Content));
                        policy.Requirements.Add(new ContentPermissionRequirement(ActionDelete.Instance.Letter.ToString()));
                    },
                    customPolicyCallback);

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MediaRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Media));
                    },
                    customPolicyCallback);

                AddAuthorizationPolicy(options,
                    AuthorizationPolicies.MemberRead,
                    policy =>
                    {
                        policy.RequireSessionIdOrRestApiClaim();
                        policy.Requirements.Add(new UmbracoSectionAccessRequirement(Core.Constants.Applications.Members));
                    },
                    customPolicyCallback);

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

        /// <summary>
        /// This will allow a custom callback to specify a custom policy if required, otherwise it will use the default policy
        /// </summary>
        /// <param name="options"></param>
        /// <param name="name"></param>
        /// <param name="configurePolicy"></param>
        /// <param name="customPolicyCallback"></param>
        public static void AddAuthorizationPolicy(
            AuthorizationOptions options,
            string name, 
            Action<AuthorizationPolicyBuilder> configurePolicy, 
            Func<string, AuthorizationPolicy, Action<AuthorizationPolicyBuilder>> customPolicyCallback)
        {
            //get the default policy
            var defaultPolicy = GetPolicy(configurePolicy);

            //check if there's a callback and use it
            if (customPolicyCallback != null)
            {
                var result = customPolicyCallback(name, defaultPolicy);
                if (result != null)
                {
                    options.AddPolicy(name, result);
                    return;
                }
            }

            //no custom callback or it didn't return a custom policy so use the default
            options.AddPolicy(name, defaultPolicy);
        }

        /// <summary>
        /// Returns a concrete policy from a builder
        /// </summary>
        /// <param name="configurePolicy"></param>
        /// <returns></returns>
        private static AuthorizationPolicy GetPolicy(Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            if (configurePolicy == null) throw new ArgumentNullException(nameof(configurePolicy));
            var authorizationPolicyBuilder = new AuthorizationPolicyBuilder();
            configurePolicy(authorizationPolicyBuilder);
            return authorizationPolicyBuilder.Build();
        }
    }
}
