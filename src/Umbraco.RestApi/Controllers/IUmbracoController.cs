using System.Net.Http;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    internal interface IUmbracoController<in TRepresentation> 
        where TRepresentation : ContentRepresentationBase
    {        
        HttpResponseMessage Get(int id);
        HttpResponseMessage Search(PagedQuery query);
        HttpResponseMessage Post(TRepresentation content);
        HttpResponseMessage Put(int id, TRepresentation content);
        HttpResponseMessage Delete(int id);
    }
}