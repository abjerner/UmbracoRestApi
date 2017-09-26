using System;
using System.Security.Claims;
using Microsoft.Owin.Security.Authorization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.RestApi.Security
{
    internal static class AuthorizationExtensions
    {
        /// <summary>
        /// A policy check for all endpoints to require either the RestApiClaimType or Umbraco's SessionIdClaimType with the Umbraco issuer
        /// </summary>
        /// <param name="policy"></param>
        public static void RequireSessionIdOrRestApiClaim(this AuthorizationPolicyBuilder policy)
        {
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    //to read published content the logged in user must have either of these claim types and value
                        c.Type == AuthorizationPolicies.RestApiClaimType
                        //if we are checking the SessionIdClaimType then it should be issued from Umbraco (i.e. cookie authentication)
                        || (c.Type == Core.Constants.Security.SessionIdClaimType && c.Issuer == UmbracoBackOfficeIdentity.Issuer)));
        }

        /// <summary>
        /// Returns the users calculated start node ids from it's claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int[] GetContentStartNodeIds(this ClaimsPrincipal user)
        {
            var startContentId = user.FindFirst(Constants.Security.StartContentNodeIdClaimType);
            if (startContentId == null || startContentId.Value.DetectIsJson() == false)
            {
                return null;
            }
            
            try
            {
                return JsonConvert.DeserializeObject<int[]>(startContentId.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}