using System.Net.Http;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    internal interface ISearchController
    {
        HttpResponseMessage Search(PagedQuery query);
    }
}