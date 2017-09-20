using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Examine;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using System.Web.Http.ModelBinding;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/content")]
    public class ContentController : UmbracoHalController
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ContentController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        public ContentController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper, 
            BaseSearchProvider searchProvider)
            : base(umbracoContext, umbracoHelper)
        {
            _searchProvider = searchProvider ?? throw new ArgumentNullException("searchProvider");
        }

        private BaseSearchProvider _searchProvider;
        protected BaseSearchProvider SearchProvider => _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"]);

        [HttpGet]
        [CustomRoute("")]
        public virtual HttpResponseMessage Get()
        {
            var rootContent = Services.ContentService.GetRootContent();
            var result = Mapper.Map<IEnumerable<ContentRepresentation>>(rootContent).ToList();
            var representation = new ContentListRepresenation(result);

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var content = Services.ContentService.GetById(id);
            var result = Mapper.Map<ContentRepresentation>(content);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id}/meta")]
        public HttpResponseMessage GetMetadata(int id)
        {
            var found = Services.ContentService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result = new ContentMetadataRepresentation(LinkTemplates.Content.MetaData, LinkTemplates.Content.Self, id)
            {
                Fields = GetDefaultFieldMetaData(),
                Properties = Mapper.Map<IDictionary<string, ContentPropertyInfo>>(found),
                CreateTemplate = Mapper.Map<ContentTemplate>(found)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result); 
        }

        [HttpGet]
        [CustomRoute("{id}/children")]
        public HttpResponseMessage GetChildren(int id,
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {
            var items = Services.ContentService.GetPagedChildren(id, query.Page, query.PageSize, out var total);
            var pages = decimal.Round(total / query.PageSize, 0);
            var mapped = Mapper.Map<IEnumerable<ContentRepresentation>>(items).ToList();

            var result = new ContentPagedListRepresentation(mapped, (int)total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Content.PagedChildren, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [HttpGet]
        [CustomRoute("{id}/descendants/")]
        public HttpResponseMessage GetDescendants(int id,
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {
            var items = Services.ContentService.GetPagedDescendants(id, query.Page, query.PageSize, out var total);
            var pages = decimal.Round(total / query.PageSize, 0);
            var mapped = Mapper.Map<IEnumerable<ContentRepresentation>>(items).ToList();

            var result = new ContentPagedListRepresentation(mapped, (int)total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Content.PagedDescendants, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id}/ancestors/")]
        public HttpResponseMessage GetAncestors(int id,
           [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {
            var items = Services.ContentService.GetAncestors(id).ToArray();
            var total = items.Length;
            var pages = decimal.Round(total / query.PageSize, 0);
            var paged = items.Skip(GetSkipSize(query.Page, query.PageSize)).Take(query.PageSize);
            var mapped = Mapper.Map<IEnumerable<ContentRepresentation>>(paged).ToList();

            var result = new ContentPagedListRepresentation(mapped, total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Content.PagedAncestors, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        
        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {

            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Query prepping - ensure that we only search for content items...
            var mediaQuery = "__IndexType:content AND " + query.Query;

            //search
            var result = SearchProvider.Search(
                    SearchProvider.CreateSearchCriteria().RawQuery(mediaQuery),
                    query.PageSize);

            //paging
            var paged = result.Skip(GetSkipSize(query.Page, query.PageSize)).ToArray();
            var pages = decimal.Round(result.TotalItemCount / query.PageSize, 0);

            var foundContent = Enumerable.Empty<IContent>();

            //Map to Imedia
            if (paged.Any())
            {
                foundContent = Services.ContentService.GetByIds(paged.Select(x => x.Id)).WhereNotNull();
            }

            //Map to representation
            var items = Mapper.Map<IEnumerable<ContentRepresentation>>(foundContent).ToList();

            //return as paged list of media items
            var representation = new ContentPagedListRepresentation(items, result.TotalItemCount, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Content.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }



        // Content CRUD:


        [HttpPost]
        [CustomRoute("")]
        public HttpResponseMessage Post(ContentRepresentation content)
        {
            if (content == null) Request.CreateResponse(HttpStatusCode.NotFound);

            try
            {
                //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Content.Root);
                }

                var contentType = Services.ContentTypeService.GetContentType(content.ContentTypeAlias);
                if (contentType == null)
                {
                    ModelState.AddModelError("content.contentTypeAlias", "No content type found with alias " + content.ContentTypeAlias);
                    throw ValidationException(ModelState, content, LinkTemplates.Content.Root);
                }

                //create an item before persisting of the correct content type
                var created = Services.ContentService.CreateContent(content.Name, content.ParentId, content.ContentTypeAlias, Security.CurrentUser.Id);

                //Validate properties
                var validator = new ContentPropertyValidator<IContent>(ModelState, Services.DataTypeService);
                validator.ValidateItem(content, created);

                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Content.Root);
                }

                Mapper.Map(content, created);
                Services.ContentService.Save(created);

                return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<ContentRepresentation>(created));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id}")]
        public HttpResponseMessage Put(int id, ContentRepresentation content)
        {
            if (content == null) Request.CreateResponse(HttpStatusCode.NotFound);

            try
            {
                var found = Services.ContentService.GetById(id);
                if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

                //Validate properties
                var validator = new ContentPropertyValidator<IContent>(ModelState, Services.DataTypeService);
                validator.ValidateItem(content, found);

                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Content.Self, id: id);
                }

                Mapper.Map(content, found);

                Services.ContentService.Save(found);

                var rep = Mapper.Map<ContentRepresentation>(found);
                return Request.CreateResponse(HttpStatusCode.OK, rep);
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        ////TODO: Check this
        //protected override IContent Publish(int id)
        //{
        //    var found = ContentService.GetById(id);
        //    if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

        //    ContentService.Publish(found, Security.CurrentUser.Id);

        //        var rep = Mapper.Map<ContentRepresentation>(found);
        //        return Request.CreateResponse(HttpStatusCode.OK, rep);
        //    }
        //    catch (ModelValidationException exception)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
        //    }
        //}

        [HttpDelete]
        [CustomRoute("{id}")]
        public virtual HttpResponseMessage Delete(int id)
        {
            var found = Services.ContentService.GetById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            Services.ContentService.Delete(found);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        
    }
   
}
