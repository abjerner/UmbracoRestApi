﻿using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class PublishedContentPagedListRepresentation : PagedListRepresentation<PublishedContentRepresentation>
    {
        public PublishedContentPagedListRepresentation(IList<PublishedContentRepresentation> res, int totalResults, int totalPages, int page, int pageSize, Link uriTemplate, object uriTemplateSubstitutionParams) : 
            base(res, totalResults, totalPages, page, pageSize, uriTemplate, uriTemplateSubstitutionParams)
        {
        }
    }
}