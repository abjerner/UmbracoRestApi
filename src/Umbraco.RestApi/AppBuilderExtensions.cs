using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Owin;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;

namespace Umbraco.RestApi
{
    public static class AppBuilderExtensions
    {
        public static void ConfigureUmbracoRestApi(this IAppBuilder app, UmbracoRestApiOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            UmbracoRestApiOptionsInstance.Options = options;
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
