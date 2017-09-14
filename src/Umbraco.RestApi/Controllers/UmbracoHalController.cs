using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    [DynamicCors]
    [UmbracoAuthorize]
    [IsBackOffice]
    [HalFormatterConfiguration]
    public abstract class UmbracoHalController : UmbracoApiControllerBase
    {

        protected UmbracoHalController()
        {
        }

        protected UmbracoHalController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        protected int CurrentVersionRequest => int.Parse(Regex.Match(Request.RequestUri.AbsolutePath, "/v(\\d+)/", RegexOptions.Compiled).Groups[1].Value);

        public int GetSkipSize(long pageIndex, int pageSize)
        {
            if (pageIndex >= 0 && pageSize > 0)
            {
                return Convert.ToInt32((pageIndex) * pageSize);
            }
            return 0;
        }

        /// <summary>
        /// Used to throw validation exceptions
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="content"></param>
        /// <param name="linkTemplate"></param>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected ModelValidationException ValidationException<TRepresentation>(
            ModelStateDictionary modelState,
            TRepresentation content,
            Link linkTemplate,
            string message = null, int? id = null, params string[] errors)
        {
            var metaDataProvider = Configuration.Services.GetModelMetadataProvider();
            var errorList = new List<ValidationErrorRepresentation>();

            foreach (KeyValuePair<string, ModelState> ms in modelState)
            {
                foreach (var error in ms.Value.Errors)
                {
                    ////hack - because webapi doesn't seem to support an easy way to change the model metadata for a class, we have to manually
                    //// go get the 'display' name from the metadata for the property and use that for the logref otherwise we end up with the c#
                    //// property name (i.e. contentTypeAlias vs ContentTypeAlias). I'm sure there's some webapi way to achieve 
                    //// this by customizing the model metadata but it's not as clear as with MVC which has IMetadataAware attribute
                    var logRef = ms.Key;
                    //var parts = ms.Key.Split('.');
                    //var isContentField = parts.Length == 2 && parts[0] == "content";
                    //if (isContentField)
                    //{
                    //    parts[1] = metaDataProvider.GetMetadataForProperty(() => content, typeof (ContentRepresentation), parts[1])
                    //                .GetDisplayName();
                    //    logRef = string.Join(".", parts);
                    //}

                    errorList.Add(new ValidationErrorRepresentation
                    {
                        LogRef = logRef,
                        Message = error.ErrorMessage
                    });
                }
            }
            
            //add additional messages
            foreach (var error in errors)
            {
                errorList.Add(new ValidationErrorRepresentation { Message = error });
            }

            var errorModel = new ValidationErrorListRepresentation(errorList, linkTemplate, id)
            {
                HttpStatus = (int)HttpStatusCode.BadRequest,
                Message = message ?? "Validation errors occurred"
            };

            return new ModelValidationException(errorModel);
        }


        [NonAction]
        protected IDictionary<string, ContentPropertyInfo> GetDefaultFieldMetaData()
        {
            //TODO: This shouldn't actually localize based on the current user!!!
            // this should localize based on the current request's Accept-Language and Content-Language headers

            return new Dictionary<string, ContentPropertyInfo>
            {
                {"id", new ContentPropertyInfo{Label = "Id", ValidationRequired = true}},
                {"key", new ContentPropertyInfo{Label = "Key", ValidationRequired = true}},
                {"contentTypeAlias", new ContentPropertyInfo{Label = TextService.Localize("content/documentType", UserCulture), ValidationRequired = true}},
                {"parentId", new ContentPropertyInfo{Label = "Parent Id", ValidationRequired = true}},
                {"hasChildren", new ContentPropertyInfo{Label = "Has Children"}},
                {"templateId", new ContentPropertyInfo{Label = TextService.Localize("template/template", UserCulture) + " Id", ValidationRequired = true}},
                {"sortOrder", new ContentPropertyInfo{Label = TextService.Localize("general/sort", UserCulture)}},
                {"name", new ContentPropertyInfo{Label = TextService.Localize("general/name", UserCulture), ValidationRequired = true}},
                {"urlName", new ContentPropertyInfo{Label = TextService.Localize("general/url", UserCulture) + " " + TextService.Localize("general/name", UserCulture)}},
                {"writerName", new ContentPropertyInfo{Label = TextService.Localize("content/updatedBy", UserCulture)}},
                {"creatorName", new ContentPropertyInfo{Label = TextService.Localize("content/createBy", UserCulture)}},
                {"writerId", new ContentPropertyInfo{Label = "Writer Id"}},
                {"creatorId", new ContentPropertyInfo{Label = "Creator Id"}},
                {"path", new ContentPropertyInfo{Label = TextService.Localize("general/path", UserCulture)}},
                {"createDate", new ContentPropertyInfo{Label = TextService.Localize("content/createDate", UserCulture)}},
                {"updateDate", new ContentPropertyInfo{Label = TextService.Localize("content/updateDate", UserCulture)}},
                {"level", new ContentPropertyInfo{Label = "Level"}},
                {"url", new ContentPropertyInfo{Label = TextService.Localize("general/url", UserCulture)}},
                {"ItemType", new ContentPropertyInfo{Label = TextService.Localize("general/type", UserCulture)}}
            };
        }

        private CultureInfo _userCulture;
        protected CultureInfo UserCulture => _userCulture ?? (_userCulture = Security.CurrentUser.GetUserCulture(TextService));

        private ILocalizedTextService TextService => Services.TextService;
    }
}
