using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.RestApi.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    public interface ICrudController<in TRepresentation> 
        where TRepresentation : Representation
    {
        Task<HttpResponseMessage> Get(int id);
        Task<HttpResponseMessage> Post(TRepresentation content);
        Task<HttpResponseMessage> Put(int id, TRepresentation content);
        Task<HttpResponseMessage> Delete(int id);
    }
}