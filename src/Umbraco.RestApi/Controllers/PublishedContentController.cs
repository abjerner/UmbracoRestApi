using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Examine;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/content/published")]
    public class PublishedContentController : UmbracoHalController
    {
        public PublishedContentController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        public PublishedContentController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper,
            BaseSearchProvider searchProvider)
            : base(umbracoContext, umbracoHelper)
        {
            _searchProvider = searchProvider ?? throw new ArgumentNullException("searchProvider");
        }

        private BaseSearchProvider _searchProvider;
        protected BaseSearchProvider SearchProvider => _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"]);


        [HttpGet]
        [CustomRoute("")]
        public virtual HttpResponseMessage Get()
        {
            var result = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>( Umbraco.TypedContentAtRoot() ).ToList();
            var representation = new PublishedContentListRepresenation(result);
            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }


        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var content = Umbraco.TypedContent(id);
            var result = AutoMapper.Mapper.Map<PublishedContentRepresentation>( content );
            
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [HttpGet]
        [CustomRoute("{id}/children")]
        public HttpResponseMessage GetChildren(int id,
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query
            )
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var resolved = (string.IsNullOrEmpty(query.Query)) ? content.Children().ToArray() : content.Children(query.Query.Split(',')).ToArray();
            var total = resolved.Length;
            var pages =  Decimal.Round( (decimal) (total / query.PageSize), 0);

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(resolved.Skip(GetSkipSize(query.Page, query.PageSize)).Take(query.PageSize)).ToList();
            var result = new PublishedContentPagedListRepresentation(items, total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.PublishedContent.PagedChildren, new { id = id });

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }   
        
        
        [HttpGet] 
        [CustomRoute("{id}/descendants/")]
        public HttpResponseMessage GetDescendants(int id,
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query
            )
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var resolved = (string.IsNullOrEmpty(query.Query)) ? content.Descendants().ToArray() : content.Descendants(query.Query).ToArray();

            var total = resolved.Length;
            var pages = Decimal.Round((decimal)(total / query.PageSize), 0);

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(resolved.Skip(GetSkipSize(query.Page, query.PageSize)).Take(query.PageSize)).ToList();
            var result = new PublishedContentPagedListRepresentation(items, total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.PublishedContent.PagedDescendants, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id}/ancestors/{page?}/{pageSize?}")]
        public HttpResponseMessage GetAncestors(int id, 
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query
            )
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var resolved = (string.IsNullOrEmpty(query.Query)) ? content.Ancestors().ToArray() : content.Ancestors(query.Query).ToArray();

            var total = resolved.Length;
            var pages = Decimal.Round((decimal)(total / query.PageSize), 0);

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(resolved.Skip(GetSkipSize(query.Page, query.PageSize)).Take(query.PageSize)).ToList();
            var result = new PublishedContentPagedListRepresentation(items, total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.PublishedContent.PagedAncestors, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [HttpGet]
        [CustomRoute("query/{id?}")]
        public HttpResponseMessage GetQuery(
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query,
            int id = 0)
        {
            var rootQuery = "";
            if (id > 0)
            {
                rootQuery = string.Format("//*[@id='{0}']", id);
            }

            var skip = query.Page * query.PageSize;
            var take = query.PageSize;

            IPublishedContent[] result = new IPublishedContent[0];

            try
            {
                result = Umbraco.TypedContentAtXPath(rootQuery + query.Query).ToArray();
            }catch(Exception e)
            {
                //in case the xpath query fails - do nothing as we will return a empty array instead    
            }

            var paged = result.Skip((int)skip).Take(take);
            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(paged).ToList();
            var representation = new PublishedContentPagedListRepresentation(items, result.Length, 1, (int)query.Page, query.PageSize, LinkTemplates.PublishedContent.Query, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        //NOTE: We cannot accept POST here for now unless we modify the routing structure since there's only one POST per
        // controller currently (with the way we've routed).
        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {
            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            //TODO: This would be more efficient if we went straight to the ExamineManager and used it's built in Skip method
            // but then we have to write our own model mappers and don't have time for that right now.
            
            var result = Umbraco.ContentQuery.TypedSearch(_searchProvider.CreateSearchCriteria().RawQuery(query.Query), _searchProvider).ToArray();
            var paged = result.Skip(GetSkipSize(query.Page, query.PageSize)).Take(query.PageSize);

            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(paged).ToList();
            var representation = new PublishedContentPagedListRepresentation(items, result.Length,  1, (int)query.Page, query.PageSize, LinkTemplates.PublishedContent.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("url")]
        public HttpResponseMessage GetByUrl(string url)
        {
            var content = UmbracoContext.ContentCache.GetByRoute(url);
            var result = AutoMapper.Mapper.Map<PublishedContentRepresentation>(content);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("tag/{tag}")]
        public HttpResponseMessage GetByTag(string tag, string group = null, int page = 0, int size = 100)
        {
            var content = Umbraco.TagQuery.GetContentByTag(tag, group).ToArray();
            var skip = (page * size);
            var total = content.Length;
            var pages = Decimal.Round((decimal)(total / size), 0);
            
            var items = AutoMapper.Mapper.Map<IEnumerable<PublishedContentRepresentation>>(content.Skip(skip).Take(size)).ToList();
            var representation = new PublishedContentPagedListRepresentation(items, total, (int)pages, page, size, LinkTemplates.PublishedContent.Search, new
            {
                tag = tag, group = group, page= page, size = size
            });

            return Request.CreateResponse(HttpStatusCode.OK, representation); 
        }

    }
}
