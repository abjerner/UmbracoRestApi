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

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/members")]
    public class MembersController : UmbracoHalController
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

        //TODO: Remove this
        [HttpPost]
        [CustomRoute("login")]
        public HttpResponseMessage Login(MemberLogin login)
        {
            try
            {
                if (Members.Login(login.Username, login.Password))
                {
                    // TODO: There must be a better way ?
                    var member = Services.MemberService.GetByUsername(login.Username);
                    var rep = Mapper.Map<MemberRepresentation>(member);
                    return Request.CreateResponse(HttpStatusCode.OK, rep);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }
        
        [HttpGet]
        [CustomRoute("")]
        public HttpResponseMessage Get(long page = 0, int size = 100, string orderBy = "Name", string direction = "Ascending", string memberTypeAlias = null, string filter = "")
        {
            long totalRecords = 0;
            var direction_enum = Enum<Core.Persistence.DatabaseModelDefinitions.Direction>.Parse(direction);
            var members = Services.MemberService.GetAll(page, size, out totalRecords, orderBy, direction_enum, memberTypeAlias, filter);
            int totalPages = ((int)totalRecords + size - 1) / size;

            var mapped = Mapper.Map<IEnumerable<MemberRepresentation>>(members).ToList();

            var representation = new MemberPagedListRepresentation(mapped, (int)totalRecords, totalPages, (int)page, size, LinkTemplates.Members.Root, new { });
            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {

            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

           
            //search
            var result = SearchProvider.Search(
                SearchProvider.CreateSearchCriteria().RawQuery(query.Query),
                query.PageSize);

            //paging
            var paged = result.Skip(GetSkipSize(query.Page, query.PageSize)).ToArray();
            var pages = Decimal.Round((decimal)(result.TotalItemCount / query.PageSize), 0);

            var foundContent = Enumerable.Empty<IMedia>();

            //Map to Imedia
            if (paged.Any())
            {
                foundContent = Services.MediaService.GetByIds(paged.Select(x => x.Id)).WhereNotNull();
            }

            //Map to representation
            var items = AutoMapper.Mapper.Map<IEnumerable<MediaRepresentation>>(foundContent).ToList();

            //return as paged list of media items
            var representation = new MediaPagedListRepresentation(items, result.TotalItemCount, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Media.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var member = Services.MemberService.GetById(id); 
            var result = AutoMapper.Mapper.Map<MemberRepresentation>(member);

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