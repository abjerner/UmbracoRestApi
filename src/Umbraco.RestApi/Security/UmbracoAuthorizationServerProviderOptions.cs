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

        /// <summary>
        /// Generally you wouldn't allow this unless on SSL!
        /// </summary>
        public bool AllowInsecureHttp { get; set; }

        /// <summary>
        /// This is the key for the client
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// This is a "ClientId"
        /// </summary>
        public string Audience { get; set; }

        public string AuthEndpoint { get; set; } = "/umbraco/rest/oauth/token";
        public CorsPolicy CorsPolicy { get; set; }
    }
}
