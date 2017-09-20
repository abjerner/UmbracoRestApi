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
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/media")]
    public class MediaController : UmbracoHalController
    {
        
        /// <summary>
        /// Default ctor
        /// </summary>
        public MediaController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        /// <param name="contentSectionConfig"></param>
        public MediaController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper,
            BaseSearchProvider searchProvider,
            IContentSection contentSectionConfig)
            : base(umbracoContext, umbracoHelper)
        {
            if (searchProvider == null) throw new ArgumentNullException("searchProvider");
            _searchProvider = searchProvider;
            _contentSectionConfig = contentSectionConfig;
        }

        private BaseSearchProvider _searchProvider;
        private IContentSection _contentSectionConfig;

        protected IContentSection ContentSectionConfig => _contentSectionConfig ?? (_contentSectionConfig = UmbracoConfig.For.UmbracoSettings().Content);
        protected BaseSearchProvider SearchProvider => _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"]);

        [HttpGet]
        [CustomRoute("")]
        public virtual HttpResponseMessage Get()
        {
            var rootMedia = Services.MediaService.GetRootMedia();
            var result = Mapper.Map<IEnumerable<MediaRepresentation>>(rootMedia).ToList();
            var representation = new MediaListRepresenation(result);

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }



        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var content = Services.MediaService.GetById(id);
            var result = Mapper.Map<MediaRepresentation>(content);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [CustomRoute("{id}/meta")]
        public HttpResponseMessage GetMetadata(int id)
        {
            var found = Services.MediaService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result = new ContentMetadataRepresentation(LinkTemplates.Media.MetaData, LinkTemplates.Media.Self, id)
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
            QueryStructure query
            )
        {
            var items = Services.MediaService.GetPagedChildren(id, query.Page, query.PageSize, out var total);
            var pages = decimal.Round(total / query.PageSize, 0);
            var mapped = Mapper.Map<IEnumerable<MediaRepresentation>>(items).ToList();

            var result = new MediaPagedListRepresentation(mapped, 
                                                            (int)total, 
                                                            (int)pages, 
                                                            (int)query.Page, 
                                                            query.PageSize, LinkTemplates.Media.PagedChildren, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [HttpGet]
        [CustomRoute("{id}/descendants/")]
        public HttpResponseMessage GetDescendants(int id,
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query
            )
        {
            var items = Services.MediaService.GetPagedDescendants(id, query.Page, query.PageSize, out var total);
            var pages = decimal.Round(total / query.PageSize, 0);
            var mapped = Mapper.Map<IEnumerable<MediaRepresentation>>(items).ToList();
            
            var result = new MediaPagedListRepresentation(mapped, (int)total, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Media.PagedDescendants, new { id = id });
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        
        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {
            if (query.Query.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Query prepping - ensure that we only search for media items...
            var mediaQuery = "__IndexType:media AND " + query.Query;

            //search
            var result = SearchProvider.Search(
                    SearchProvider.CreateSearchCriteria().RawQuery(mediaQuery),
                    query.PageSize);

            //paging
            var paged = result.Skip( GetSkipSize(query.Page, query.PageSize)).ToArray();
            var pages = decimal.Round(result.TotalItemCount / query.PageSize, 0);

            var foundContent = Enumerable.Empty<IMedia>();
            
            //Map to Imedia
            if (paged.Any())
            {
                foundContent = Services.MediaService.GetByIds(paged.Select(x => x.Id)).WhereNotNull();
            }
            
            //Map to representation
            var items = Mapper.Map<IEnumerable<MediaRepresentation>>(foundContent).ToList();

            //return as paged list of media items
            var representation = new MediaPagedListRepresentation(items, result.TotalItemCount, (int)pages, (int)query.Page, query.PageSize, LinkTemplates.Media.Search, new { query = query.Query, pageSize = query.PageSize });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }



        // Media CRUD:
        

        [HttpPost]
        [CustomRoute("")]
        public HttpResponseMessage Post(MediaRepresentation content)
        {
            if (content == null) Request.CreateResponse(HttpStatusCode.NotFound);

            try
            {
                //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Media.Root);
                }

                var contentType = Services.ContentTypeService.GetMediaType(content.ContentTypeAlias);
                if (contentType == null)
                {
                    ModelState.AddModelError("content.contentTypeAlias", "No media type found with alias " + content.ContentTypeAlias);
                    throw ValidationException(ModelState, content, LinkTemplates.Media.Root);
                }

                //create an item before persisting of the correct content type
                var created = Services.MediaService.CreateMedia(content.Name, content.ParentId, content.ContentTypeAlias, Security.CurrentUser.Id);

                //Validate properties
                var validator = new ContentPropertyValidator<IMedia>(ModelState, Services.DataTypeService);
                validator.ValidateItem(content, created);

                if (!ModelState.IsValid)
                {
                    throw ValidationException(ModelState, content, LinkTemplates.Media.Root);
                }

                Mapper.Map(content, created);
                Services.MediaService.Save(created);
                
                return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<MediaRepresentation>(created) );
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id}")]
        public HttpResponseMessage Put(int id, RelationRepresentation content)
        {
            if (content == null) Request.CreateResponse(HttpStatusCode.NotFound);

            try
            {
                var found = Services.MediaService.GetById(id);
                if (found == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                Mapper.Map(content, found);
                Services.MediaService.Save(found);

                var rep = Mapper.Map<MediaRepresentation>(found);
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
            var found = Services.MediaService.GetById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            Services.MediaService.Delete(found);
            return Request.CreateResponse(HttpStatusCode.OK);
        }



        [HttpPut]
        [CustomRoute("{id}/upload")]
        public async Task<HttpResponseMessage> UploadFile(int id, string property = "umbracoFile")
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "The request doesn't contain valid content!");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.Contents.Count != 1)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This method only works with a single file at a time");

            var file = (StreamContent)provider.Contents.First();
            var name = file.Headers.ContentDisposition.FileName;
            var contentType = file.Headers.ContentType;
            var dataStream = await file.ReadAsStreamAsync();

            //build an in-memory file for umbraco
            var httpFile = new MemoryFile(dataStream, contentType.ToString(), name);

            var media = Services.MediaService.GetById(id);
            if (media == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            media.SetValue(property, httpFile);
            Services.MediaService.Save(media);

            return Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<MediaRepresentation>(media));
        }

        [HttpPost]
        [CustomRoute("{id}/upload")]
        public async Task<HttpResponseMessage> PostFile(int id, string mediaType = null, string property = Constants.Conventions.Media.File)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "The request doesn't contain valid content!");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.Contents.Count != 1)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This method only works with a single file at a time");

            var file = (StreamContent)provider.Contents.First();
            var name = file.Headers.ContentDisposition.FileName;
            var safeFileName = file.Headers.ContentDisposition.FileName.ToSafeFileName();
            var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

            if (ContentSectionConfig.IsFileAllowedForUpload(ext) == false)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Files of this type not allowed");

            if (string.IsNullOrEmpty(mediaType))
            {
                mediaType = Constants.Conventions.MediaTypes.File;
                if (ContentSectionConfig.ImageFileTypes.Contains(ext))
                {
                    mediaType = Constants.Conventions.MediaTypes.Image;
                }
            }

            var contentType = file.Headers.ContentType;
            var dataStream = await file.ReadAsStreamAsync();

            //build an in-memory file for umbraco
            var httpFile = new MemoryFile(dataStream, contentType.ToString(), name);

            var media = Services.MediaService.CreateMedia(name, id, mediaType);
            media.SetValue(property, httpFile);
            Services.MediaService.Save(media);
            return Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<MediaRepresentation>(media));
        }
    }

}
