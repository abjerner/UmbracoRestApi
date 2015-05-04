﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Examine.Providers;
using Umbraco.Core.Models;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// REST service for querying against Published content
    /// </summary>    
    public class PublishedContentController : UmbracoHalController<int, IPublishedContent>
    {
        //TODO: We need to make a way to return IPublishedContent from either the cache or from Examine, then convert that to the output
        // this controller needs to support both data sources in one way or another - either base classes, etc...

        /// <summary>
        /// Default ctor
        /// </summary>
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
            : base(umbracoContext, umbracoHelper, searchProvider)
        {
        }

        protected override IEnumerable<IPublishedContent> GetRootContent()
        {
            return Umbraco.TypedContentAtRoot();
        }

        protected override ContentMetadataRepresentation GetMetadataForItem(int id)
        {
            var found = Umbraco.TypedContent(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result = new ContentMetadataRepresentation(LinkTemplate, id)
            {
                Fields = GetDefaultFieldMetaData(),
                // NOTE: we cannot determine this from IPublishedContent
                Properties = null, 
                // NOTE: null because IPublishedContent is readonly
                CreateTemplate = null
            };
            return result;
        }

        protected override IPublishedContent GetItem(int id)
        {
            return Umbraco.TypedContent(id);
        }

        protected override PagedResult<IPublishedContent> GetChildContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            var resolved = content.Children.ToArray();

            return new PagedResult<IPublishedContent>(resolved.Length, pageIndex + 1, pageSize)
            {
                Items = resolved.Skip(GetSkipSize(pageIndex, pageSize)).Take(pageSize)
            };
        }

        protected override PagedResult<IPublishedContent> GetDescendantContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            var resolved = content.Descendants().ToArray();

            return new PagedResult<IPublishedContent>(resolved.Length, pageIndex + 1, pageSize)
            {
                Items = resolved.Skip(GetSkipSize(pageIndex, pageSize)).Take(pageSize)
            };
        }

        protected override IContentLinkTemplate LinkTemplate
        {
            get { return new PublishedContentLinkTemplate(CurrentVersionRequest); }
        }
    }
}