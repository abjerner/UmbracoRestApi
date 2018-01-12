using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Models.Mapping
{
    public class RelationModelMapper : MapperConfiguration
    {
        
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRelation, RelationRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.Id, expression => expression.MapFrom(x => x.Key))
                .ForMember(
                    representation => representation.ChildId,
                    expression => expression.ResolveUsing(
                        new RelationKeyResolver(RelationDirection.Child, applicationContext.Services.EntityService)))
                .ForMember(
                    representation => representation.ParentId,
                    expression => expression.ResolveUsing(
                        new RelationKeyResolver(RelationDirection.Parent, applicationContext.Services.EntityService)))
                .ForMember(representation => representation.CreateDate, expression => expression.MapFrom(x => x.CreateDate.ToUniversalTime()))
                .ForMember(representation => representation.UpdateDate, expression => expression.MapFrom(x => x.UpdateDate.ToUniversalTime()))
                .ForMember(representation => representation.RelationTypeAlias, expression => expression.MapFrom(member => member.RelationType.Alias));
            
            config.CreateMap<IRelationType, RelationTypeRepresentation>()
                .IgnoreHalProperties()
                .ForMember(rep => rep.ParentEntityType, ex => ex.ResolveUsing(content => ConvertGuidToPublishedType(content.ParentObjectType)))
                .ForMember(rep => rep.ChildEntityType, ex => ex.ResolveUsing(content => ConvertGuidToPublishedType(content.ChildObjectType)));

            config.CreateMap<RelationRepresentation, IRelation>()
                .ConstructUsing((RelationRepresentation source) =>
                {
                    //TODO: For this to work we need to modify Core to try to fetch an Id for a Key without an object type
                    //var intParentId = applicationContext.Services.EntityService.GetIdForKey(source.ParentId);
                    //var intChildId = applicationContext.Services.EntityService.GetIdForKey(source.ChildId);
                    //return new Relation(intParentId.Result, intChildId.Result, applicationContext.Services.RelationService.GetRelationTypeByAlias(source.RelationTypeAlias));
                    return null;
                })
                .ForMember(dto => dto.DeletedDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())
                .ForMember(dto => dto.Key, expression => expression.Ignore())
                .ForMember(dto => dto.ParentId, expression => expression.Ignore())  //ignored because this is set in the ctor
                .ForMember(dto => dto.ChildId, expression => expression.Ignore())   //ignored because this is set in the ctor
                .ForMember(dto => dto.RelationType, expression => expression.MapFrom(x => applicationContext.Services.RelationService.GetRelationTypeByAlias(x.RelationTypeAlias)))
                .ForMember(dto => dto.Id, expression => expression.ResolveUsing(new RelationIdResolver(applicationContext.Services.EntityService)))
                .ForMember(dest => dest.Id, expression => expression.Condition(representation => (representation.Id != Guid.Empty))); //don't map if it's empty (i.e. it's new)
        }

        private enum RelationDirection
        {
            Child, Parent
        }

        private class RelationIdResolver : ValueResolver<RelationRepresentation, int>
        {
            private readonly IEntityService _entityService;

            public RelationIdResolver(IEntityService entityService)
            {
                _entityService = entityService;
            }

            protected override int ResolveCore(RelationRepresentation source)
            {
                //TODO: For this to work we need to modify Core to try to fetch an Key for an ID for a relation (which there is zero support for currently)
                //var attempt = _entityService.GetIdForKey(source.Id, Relation);
                
                return 0;
            }
        }

        private class RelationKeyResolver : ValueResolver<IRelation, Guid>
        {
            private readonly RelationDirection _dir;
            private readonly IEntityService _entityService;

            public RelationKeyResolver(RelationDirection dir, IEntityService entityService)
            {
                _dir = dir;
                _entityService = entityService;
            }

            protected override Guid ResolveCore(IRelation source)
            {
                //TODO: For this to work we need to modify Core to try to fetch an Key for an ID without an object type
                //var attempt = _entityService.GetKeyForId(_dir == RelationDirection.Parent ? source.ParentId : source.ChildId);
                //return attempt ? attempt.Result : Guid.Empty;
                return Guid.Empty;
            }
        }

        private static PublishedItemType ConvertGuidToPublishedType(Guid guid)
        {
            if (guid == Constants.ObjectTypes.DocumentGuid)
                return PublishedItemType.Content;

            if (guid == Constants.ObjectTypes.MediaGuid)
                return PublishedItemType.Media;

            if (guid == Constants.ObjectTypes.MemberGuid)
                return PublishedItemType.Member;
            
            //default return value
            return PublishedItemType.Content;
        } 
        
    }
}
