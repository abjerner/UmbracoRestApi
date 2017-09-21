using System.Net.Http;
using Umbraco.RestApi.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    internal interface ICrudController<in TRepresentation> 
        where TRepresentation : Representation
    {
        HttpResponseMessage Get(int id);
        HttpResponseMessage Post(TRepresentation content);
        HttpResponseMessage Put(int id, TRepresentation content);
        HttpResponseMessage Delete(int id);
    }
}