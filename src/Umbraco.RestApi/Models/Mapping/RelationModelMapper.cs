using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.RestApi.Models.Mapping
{
    public class RelationModelMapper : MapperConfiguration
    {
        
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRelation, RelationRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.RelationTypeAlias, expression => expression.MapFrom(member => member.RelationType.Alias));
            
            config.CreateMap<IRelationType, RelationTypeRepresentation>()
                .IgnoreHalProperties()
                .ForMember(rep => rep.ParentEntityType, ex => ex.ResolveUsing(content => ConvertGuidToPublishedType(content.ParentObjectType)))
                .ForMember(rep => rep.ChildEntityType, ex => ex.ResolveUsing(content => ConvertGuidToPublishedType(content.ChildObjectType)));

            config.CreateMap<RelationRepresentation, IRelation>()
                .ConstructUsing(source => new Relation(source.ParentId, source.ChildId, applicationContext.Services.RelationService.GetRelationTypeByAlias(source.RelationTypeAlias)))
                .ForMember(dto => dto.DeletedDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())
                .ForMember(dto => dto.Key, expression => expression.Ignore())                
                .ForMember(dto => dto.RelationType, expression => expression.MapFrom(x => applicationContext.Services.RelationService.GetRelationTypeByAlias(x.RelationTypeAlias)))
                .ForMember(dest => dest.Id, expression => expression.Condition(representation => (representation.Id > 0)));
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
