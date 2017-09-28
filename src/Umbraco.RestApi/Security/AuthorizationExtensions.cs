using System;
using System.Security.Claims;
using System.Web.Http.Filters;
using Microsoft.Owin.Security.Authorization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Security
{
    internal static class AuthorizationExtensions
    {
        /// <summary>
        /// Looks up the IUser instance based on the id specified in claims
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        public static IUser GetUserFromClaims(this ClaimsPrincipal principal, IUserService userService)
        {
            var idClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier && c.Issuer == UmbracoBackOfficeIdentity.Issuer);
            if (idClaim == null)
            {
                return null;
            }
            var id = idClaim.Value.TryConvertTo<int>();
            if (!id)
            {
                return null;
            }
            var user = userService.GetUserById(id.Result);
            return user;
        }

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

        /// <summary>
        /// Returns the users calculated start node ids from it's claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int[] GetMediaStartNodeIds(this ClaimsPrincipal user)
        {
            var startMediaId = user.FindFirst(Constants.Security.StartMediaNodeIdClaimType);
            if (startMediaId == null || startMediaId.Value.DetectIsJson() == false)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<int[]>(startMediaId.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}