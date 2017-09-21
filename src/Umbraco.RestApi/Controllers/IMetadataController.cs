using System.Net.Http;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    internal interface IMetadataController
    {
        HttpResponseMessage GetMetadata(int id);
    }
}