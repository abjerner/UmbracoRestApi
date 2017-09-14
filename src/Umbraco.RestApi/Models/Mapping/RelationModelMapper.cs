using AutoMapper;
using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.RestApi.Models
{
    class RelationModelMapper : MapperConfiguration
    {
        
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRelation, RelationRepresentation>()
                .ForMember(representation => representation.RelationTypeAlias, expression => expression.MapFrom(member => member.RelationType.Alias));


            config.CreateMap<IRelationType, RelationTypeRepresentation>()
                .ForMember(rep => rep.ParentEntityType, ex => ex.ResolveUsing(content => convertGuidToPublishedType(content.ParentObjectType)))
                .ForMember(rep => rep.ChildEntityType, ex => ex.ResolveUsing(content => convertGuidToPublishedType(content.ChildObjectType)));



            config.CreateMap<RelationRepresentation, IRelation>()
                .ConstructUsing((RelationRepresentation source) => new Relation(source.ParentId, source.ChildId, ApplicationContext.Current.Services.RelationService.GetRelationTypeByAlias(source.RelationTypeAlias)))

                .ForMember(dto => dto.ParentId, expression => expression.Ignore())
                .ForMember(dto => dto.ChildId, expression => expression.Ignore())

                .ForMember(dest => dest.Id, expression => expression.Condition(representation => (representation.Id > 0)));
        }


        private PublishedItemType convertGuidToPublishedType(Guid guid)
        {
            var id = guid.ToString();

            if (id == Core.Constants.ObjectTypes.ContentItem)
                return PublishedItemType.Content;

            if (id == Core.Constants.ObjectTypes.Media)
                return PublishedItemType.Media;

            if (id == Core.Constants.ObjectTypes.Member)
                return PublishedItemType.Member;


            //default return value
            return PublishedItemType.Content;
        } 
        
    }
}
