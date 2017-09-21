using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using Umbraco.Core;
using System.Linq;
using Examine;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using Examine.Providers;
using Umbraco.Web.WebApi;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoAuthorize]
    [UmbracoRoutePrefix("rest/v1/members")]
    public class MembersController : UmbracoHalController, IUmbracoController<MemberRepresentation>
    {
        public MembersController()
        {
        }

        public MembersController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper, BaseSearchProvider searchProvider)
            : base(umbracoContext, umbracoHelper)
        {
            if (searchProvider == null) throw new ArgumentNullException("searchProvider");
            _searchProvider = searchProvider;
        }

        private BaseSearchProvider _searchProvider;
        protected BaseSearchProvider SearchProvider => _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"]);
        
        [HttpGet]
        [CustomRoute("")]
        public HttpResponseMessage Get(
            [ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query,
            string orderBy = "Name", string direction = "Ascending", string memberTypeAlias = null)
        {
            var directionEnum = Enum<Core.Persistence.DatabaseModelDefinitions.Direction>.Parse(direction);
            var members = Services.MemberService.GetAll(query.Page - 1, query.PageSize, out var totalRecords, orderBy, directionEnum, memberTypeAlias, query.Query);
            var totalPages = ContentControllerHelper.GetTotalPages(totalRecords, query.PageSize);            

            var mapped = Mapper.Map<IEnumerable<MemberRepresentation>>(members).ToList();

            var representation = new MemberPagedListRepresentation(mapped, totalRecords, totalPages, query.Page, query.PageSize, LinkTemplates.Members.Root, new { });
            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [ModelBinder(typeof(PagedQueryModelBinder))]
            PagedQuery query)
        {

            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

           
            //search
            var result = SearchProvider.Search(
                SearchProvider.CreateSearchCriteria().RawQuery(query.Query),
                query.PageSize);

            //paging
            var paged = result.Skip(ContentControllerHelper.GetSkipSize(query.Page - 1, query.PageSize)).ToArray();
            var pages = (result.TotalItemCount + query.PageSize - 1) / query.PageSize;

            var foundContent = Enumerable.Empty<IMedia>();

            //Map to Imedia
            if (paged.Any())
            {
                foundContent = Services.MediaService.GetByIds(paged.Select(x => x.Id)).WhereNotNull();
            }

            //Map to representation
            var items = Mapper.Map<IEnumerable<MediaRepresentation>>(foundContent).ToList();

            //return as paged list of media items
            var representation = new MediaPagedListRepresentation(items, result.TotalItemCount, pages, query.Page - 1, query.PageSize, LinkTemplates.Media.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var member = Services.MemberService.GetById(id); 
            var result = Mapper.Map<MemberRepresentation>(member);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }


        // Content CRUD:


        [HttpPost]
        [CustomRoute("")]
        public HttpResponseMessage Post(MemberRepresentation content)
        {
            if (content == null) Request.CreateResponse(HttpStatusCode.NotFound);

            try
            {
                //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Members.Root);
                }

                var contentType = Services.MemberTypeService.Get(content.ContentTypeAlias);
                if (contentType == null)
                {
                    ModelState.AddModelError("content.contentTypeAlias", "No member type found with alias " + content.ContentTypeAlias);
                    throw ValidationException(ModelState, content, LinkTemplates.Members.Root);
                }

                //create an item before persisting of the correct content type
                var created = Services.MemberService.CreateMember(content.Email, content.Email, content.Name, content.ContentTypeAlias);

                //Validate properties
                var validator = new ContentPropertyValidator<IMember>(ModelState, Services.DataTypeService);
                validator.ValidateItem(content, created);

                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Members.Root);
                }

                Mapper.Map(content, created);
                Services.MemberService.Save(created);

                return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<MemberRepresentation>(created));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id}")]
        public HttpResponseMessage Put(int id, MemberRepresentation content)
        {
            try
            {
                var found = Services.MemberService.GetById(id);
                if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

                //Validate properties
                var validator = new ContentPropertyValidator<IMember>(ModelState, Services.DataTypeService);
                validator.ValidateItem(content, found);

                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Members.Self, id: id);
                }

                Mapper.Map(content, found);

                Services.MemberService.Save(found);

                var rep = Mapper.Map<MemberRepresentation>(found);
                return Request.CreateResponse(HttpStatusCode.OK, rep);
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpDelete]
        [CustomRoute("{id}")]
        public virtual HttpResponseMessage Delete(int id)
        {
            var found = Services.MemberService.GetById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            Services.MemberService.Delete(found);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    
    }
}