using System.Net.Http;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    internal interface IRootController
    {
        /// <summary>
        /// Returns the items at the root
        /// </summary>
        /// <returns></returns>
        HttpResponseMessage Get();
    }
}