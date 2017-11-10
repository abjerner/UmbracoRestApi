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
                .ForMember(representation => representation.CreateDate, expression => expression.MapFrom(x => x.CreateDate.ToUniversalTime())) 
                .ForMember(representation => representation.UpdateDate, expression => expression.MapFrom(x => x.UpdateDate.ToUniversalTime())) 
                .ForMember(representation => representation.Key, expression => expression.MapFrom(x => (x is IPublishedContentWithKey) ? ((IPublishedContentWithKey) x).Key : Guid.Empty))
                .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content => content.Children.Any()))
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing((ResolutionResult result) =>
                {
                    var content = (IPublishedContent) result.Context.SourceValue;

                    //Check the context for our special value - this allows us to only render one level of recursive IPublishedContent properties,
                    //since we don't want to cause it to render tons of nested picked properties, just the first level
                    result.Context.Options.Items.TryGetValue("prop::level", out var level);

                    var d = content.Properties.ToDictionary(property => property.PropertyTypeAlias, property =>
                    {
                        if (property.Value is IPublishedContent)
                        {
                            //if a level is set then exit, we don't want to process deeper than one level
                            if (level != null) return null;
                            //re-map but pass in a level so this recursion doesn't continue
                            return Mapper.Map<PublishedContentRepresentation>(property.Value, options => options.Items["prop::level"] = 1);
                        }

                        if (property.Value is IEnumerable<IPublishedContent>)
                        {
                            //if a level is set then exit, we don't want to process deeper than one level
                            if (level != null) return null;
                            //re-map but pass in a level so this recursion doesn't continue
                            return Mapper.Map<IEnumerable<PublishedContentRepresentation>>(property.Value, options => options.Items["prop::level"] = 1);
                        }

                        return property.Value;
                    });

                    return d;
                }));
        }
    }
}
