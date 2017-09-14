using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public abstract class PagedListRepresentation<TRepresentation> : SimpleListRepresentation<TRepresentation> where TRepresentation : Representation
    {
        private readonly Link _uriTemplate;

        protected PagedListRepresentation(IList<TRepresentation> res, int totalResults, int totalPages, int page, int pageSize, Link uriTemplate, object uriTemplateSubstitutionParams)
            : base(res)
        {
            _uriTemplate = uriTemplate;
            TotalResults = totalResults;
            TotalPages = totalPages;
            PageIndex = page;
            PageSize = pageSize;
            UriTemplateSubstitutionParams = uriTemplateSubstitutionParams;
        }

        public int TotalResults { get; set; }
        public int TotalPages { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        protected object UriTemplateSubstitutionParams;

        protected override void CreateHypermedia()
        {
            var prms = new List<object> { new { page = PageIndex, size = PageSize } };
            if (UriTemplateSubstitutionParams != null)
                prms.Add(UriTemplateSubstitutionParams);

            Href = Href ?? _uriTemplate.CreateLink(prms.ToArray()).Href;

            Links.Add(new Link { Href = Href, Rel = "self" });


            if (PageIndex > 0)
            {
                var item = UriTemplateSubstitutionParams == null
                                ? _uriTemplate.CreateLink("prev", new { page = PageIndex - 1, size = PageSize })
                                : _uriTemplate.CreateLink("prev", UriTemplateSubstitutionParams, new { page = PageIndex - 1, size = PageSize }); // page overrides UriTemplateSubstitutionParams
                Links.Add(item);
            }

            if (PageIndex < (TotalPages - 1))
            {
                var link = UriTemplateSubstitutionParams == null // kbr
                               ? _uriTemplate.CreateLink("next", new { page = PageIndex + 1, size = PageSize })
                               : _uriTemplate.CreateLink("next", UriTemplateSubstitutionParams, new { page = PageIndex + 1, size = PageSize }); // page overrides UriTemplateSubstitutionParams
                Links.Add(link);
            }

            Links.Add(new Link("page", _uriTemplate.Href));
        }
    }
}
