using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    //TODO: How to authorize this? https://github.com/umbraco/UmbracoRestApi/issues/24
    [UmbracoAuthorize]
    [UmbracoRoutePrefix("rest/v1/relations")]
    public class RelationsController : UmbracoHalController, ICrudController<RelationRepresentation>, IRootController
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public RelationsController()
        {

        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        public RelationsController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        { }

        [HttpGet]
        [CustomRoute("")]
        public HttpResponseMessage Get()
        {   
            var relationTypes = Services.RelationService.GetAllRelationTypes();
            var mapped = Mapper.Map<IEnumerable<RelationTypeRepresentation>>(relationTypes).ToList();

            var result = new RelationTypeListRepresentation(mapped);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        
        [HttpGet]
        [CustomRoute("children/{id}")]
        public HttpResponseMessage GetByParent(int id, string relationType = null)
        {
            var parent = Services.EntityService.Get(id);

            if (parent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
                
            var relations = (string.IsNullOrEmpty(relationType)) ? Services.RelationService.GetByParent(parent) : Services.RelationService.GetByParent(parent, relationType);
            var mapped = relations.Select(CreateRepresentation).ToList();

            var relationsRep = new RelationListRepresentation( mapped );
            return Request.CreateResponse(HttpStatusCode.OK, relationsRep);
        }

        [HttpGet]
        [CustomRoute("parents/{id}")]
        public HttpResponseMessage GetByChild(int id, string relationType = null)
        {
            var child = Services.EntityService.Get(id);
            if (child == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var type = Services.RelationService.GetRelationTypeByAlias(relationType);
            if (type == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);


            var relations = (string.IsNullOrEmpty(relationType)) ? Services.RelationService.GetByChild(child) : Services.RelationService.GetByChild(child, relationType);
            var mapped = relations.Select(CreateRepresentation).ToList();
            var relationsRep = new RelationListRepresentation(mapped);

            return Request.CreateResponse(HttpStatusCode.OK, relationsRep);
        }
        



        //RELATIONS CRUD
        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var result = Services.RelationService.GetById(id);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(result));
        }

        [HttpPost]
        [CustomRoute("")]
        public HttpResponseMessage Post(RelationRepresentation representation)
        {
            try
            {
                var relation = Mapper.Map<IRelation>(representation);
                Services.RelationService.Save(relation);
                return Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(relation));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id}")]
        public HttpResponseMessage Put(int id, RelationRepresentation rel)
        {
            try
            {
                var found = Services.RelationService.GetById(id);
                if (found == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                Mapper.Map(rel, found);
                Services.RelationService.Save(found);

                return Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(found));
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
            var found = Services.RelationService.GetById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            Services.RelationService.Delete(found);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        

        private RelationRepresentation CreateRepresentation(IRelation relation)
        {
            var parentLinkTemplate = GetLinkTemplate(relation.RelationType.ParentObjectType);
            var childLinkTemplate = GetLinkTemplate(relation.RelationType.ChildObjectType);

            var rep = new RelationRepresentation(parentLinkTemplate, childLinkTemplate);
            return Mapper.Map(relation, rep);
        }

        private Link GetLinkTemplate(Guid nodeObjectType)
        {
            switch (nodeObjectType.ToString().ToUpper())
            {
                case Core.Constants.ObjectTypes.ContentItem:
                    return LinkTemplates.PublishedContent.Self;

                case Core.Constants.ObjectTypes.Media:
                    return LinkTemplates.Media.Self;

                case Core.Constants.ObjectTypes.Member:
                    return LinkTemplates.Members.Self;

                default:
                    break;
            }

            return null;
        }
        
    }


}
