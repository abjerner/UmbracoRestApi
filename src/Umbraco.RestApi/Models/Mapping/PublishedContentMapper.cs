using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.RestApi.Models.Mapping
{
    public class PublishedContentMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {

            config.CreateMap<IPublishedContent, PublishedContentRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.Key, expression => expression.MapFrom(x => (x is IPublishedContentWithKey) ? ((IPublishedContentWithKey) x).Key : Guid.Empty))
                .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content => content.Children.Any()))                
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
                {
                    var result = content.Properties.ToDictionary(property => property.PropertyTypeAlias, property =>
                    {
                        //PP: Special rule - if a piece of content is pointing at a IPublished content - this is put here to make the current published API work - but it will
                        //lead to possible circular references issues when serializing... 
                        //TODO: How to deal with this? Assuming this is because of property value converters
                        if (property.Value is IPublishedContent)
                            return Mapper.Map<PublishedContentRepresentation>(property.Value);

                        if (property.Value is IEnumerable<IPublishedContent>)
                            return Mapper.Map<IEnumerable<PublishedContentRepresentation>>(property.Value);
                        
                        return property.Value;
                    });

                    return result;
                }));
        }
    }
}
