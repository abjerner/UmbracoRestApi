using System.Net.Http;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    internal interface ITraversableController<in TRepresentation> : ISearchController, ICrudController<TRepresentation>, IRootController, IMetadataController
        where TRepresentation : ContentRepresentationBase
    {
        HttpResponseMessage GetChildren(int id, PagedQuery query);

        HttpResponseMessage GetDescendants(int id, PagedQuery query);

        HttpResponseMessage GetAncestors(int id, PagedRequest query);
    }
}