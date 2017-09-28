using System.Collections.Generic;
using System.Text;
using System.Web.Cors;
using Microsoft.AspNet.Identity.Owin;

namespace Umbraco.RestApi.Security
{
    public class UmbracoAuthorizationServerProviderOptions
    {
        /// <summary>
        /// Default options allows all request, CORS does not limit anything
        /// </summary>
        public UmbracoAuthorizationServerProviderOptions()
        {
            //These are the defaults that we know work but people can modify them
            // on startup if required.
            CorsPolicy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = true,
                SupportsCredentials = true
            };
        }

        public string Secret { get; set; }
        public string Audience { get; set; }
        public string AuthEndpoint { get; set; } = "/umbraco/restapi/oauth/token";
        public CorsPolicy CorsPolicy { get; set; }
    }
}
