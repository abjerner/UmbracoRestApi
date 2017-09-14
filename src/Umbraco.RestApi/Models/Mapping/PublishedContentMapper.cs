using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.RestApi.Models
{
    public class PublishedContentMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {

            config.CreateMap<IPublishedContent, PublishedContentRepresentation>()
               .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content => content.Children.Any()))

               .ForMember(representation => representation.Rel, ex => ex.Ignore())
               .ForMember(representation => representation.Href, ex => ex.Ignore())

               .ForMember(rep => rep.CreateDate, ex => ex.ResolveUsing( content =>
               {
                   var result = content.CreateDate;
                   if (result == default(DateTime))
                       return null;

                   return result;
               }))

               .ForMember(rep => rep.UpdateDate, ex => ex.ResolveUsing(content =>
               {
                   var result = content.UpdateDate;
                   if (result == default(DateTime))
                       return null;

                   return result;
               }))

               .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
               {

                   var result = content.Properties.ToDictionary(property => property.PropertyTypeAlias, property =>
                   {

                        //PP: Special rule - if a piece of content is pointing at a IPublished content - this is put here to make the current published API work - but it will
                        //lead to possible circular references issues when serializing... 
                        if (property.Value is IPublishedContent)
                           return Mapper.Map<PublishedContentRepresentation>(property.Value);

                       if (property.Value is IEnumerable<IPublishedContent>)
                           return Mapper.Map< IEnumerable<PublishedContentRepresentation>> (property.Value);


                       return property.Value;
                   });


                   return result;
               }));
        }
    }
}
