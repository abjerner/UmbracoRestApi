using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Umbraco.RestApi.Security
{
    public class AuthorizationPolicies
    {
        public const string RestApiClaimType = "http://umbraco.org/2017/09/identity/claims/restapi";

        public const string PublishedContentRead = "PublishedContentRead";
        public const string MemberRead = "MemberRead";
        public const string MediaRead = "MediaRead";

        public const string ContentRead = "ContentRead";
        public const string ContentCreate = "ContentCreate";
        public const string ContentUpdate = "ContentUpdate";
        public const string ContentDelete = "ContentDelete";
    }
}
