using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;
using Umbraco.Core.Models;

namespace Umbraco.RestApi.Controllers
{
    public class ContentControllerHelper
    {
        private readonly ILocalizedTextService _textService;

        public ContentControllerHelper(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        internal static int GetTotalPages(long totalRecords, int pageSize)
        {
            var totalPages = ((int)totalRecords + pageSize - 1) / pageSize;
            return totalPages;
        }

        internal static int GetSkipSize(long pageIndex, int pageSize)
        {
            if (pageIndex >= 0 && pageSize > 0)
            {
                return Convert.ToInt32((pageIndex) * pageSize);
            }
            return 0;
        }

        internal IDictionary<string, ContentPropertyInfo> GetDefaultFieldMetaData(ClaimsPrincipal user)
        {
            var cultureClaim = user.FindFirst(ClaimTypes.Locality);
            if (cultureClaim == null)
                throw new InvalidOperationException($"The required user claim {ClaimTypes.Locality} is missing");

            var userCulture = new CultureInfo(cultureClaim.Value);
            //TODO: This shouldn't actually localize based on the current user!!!
            // this should localize based on the current request's Accept-Language and Content-Language headers

            return new Dictionary<string, ContentPropertyInfo>
            {
                {"id", new ContentPropertyInfo{Label = "Id", ValidationRequired = true}},
                {"key", new ContentPropertyInfo{Label = "Key", ValidationRequired = true}},
                {"contentTypeAlias", new ContentPropertyInfo{Label = _textService.Localize("content/documentType", userCulture), ValidationRequired = true}},
                {"parentId", new ContentPropertyInfo{Label = "Parent Id", ValidationRequired = true}},
                {"hasChildren", new ContentPropertyInfo{Label = "Has Children"}},
                {"templateId", new ContentPropertyInfo{Label = _textService.Localize("template/template", userCulture) + " Id", ValidationRequired = true}},
                {"sortOrder", new ContentPropertyInfo{Label = _textService.Localize("general/sort", userCulture)}},
                {"name", new ContentPropertyInfo{Label = _textService.Localize("general/name", userCulture), ValidationRequired = true}},
                {"urlName", new ContentPropertyInfo{Label = _textService.Localize("general/url", userCulture) + " " + _textService.Localize("general/name", userCulture)}},
                {"writerName", new ContentPropertyInfo{Label = _textService.Localize("content/updatedBy", userCulture)}},
                {"creatorName", new ContentPropertyInfo{Label = _textService.Localize("content/createBy", userCulture)}},
                {"writerId", new ContentPropertyInfo{Label = "Writer Id"}},
                {"creatorId", new ContentPropertyInfo{Label = "Creator Id"}},
                {"path", new ContentPropertyInfo{Label = _textService.Localize("general/path", userCulture)}},
                {"createDate", new ContentPropertyInfo{Label = _textService.Localize("content/createDate", userCulture)}},
                {"updateDate", new ContentPropertyInfo{Label = _textService.Localize("content/updateDate", userCulture)}},
                {"level", new ContentPropertyInfo{Label = "Level"}},
                {"url", new ContentPropertyInfo{Label = _textService.Localize("general/url", userCulture)}},
                //TODO: Do we use this?
                {"ItemType", new ContentPropertyInfo{Label = _textService.Localize("general/type", userCulture)}}
            };
        }
    }
}
