using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.RestApi.Models.Mapping
{
    public class PublishedContentMapper : MapperConfiguration
    {
        /// <summary>
        /// Gets the published content with a guaranteed key
        /// </summary>
        /// <param name="content">The published content</param>
        /// <returns>A published content with key</returns>
        /// <remarks>
        /// Workaround for the fact that GetKey() doesn't always work - see http://issues.umbraco.org/issue/U4-10128
        /// </remarks>
        private static IPublishedContentWithKey GetContentWithKey(IPublishedContent content)
        {
            var withKey = content as IPublishedContentWithKey;

            if (withKey != null)
                return withKey;

            var wrapped = content as PublishedContentWrapped;

            if (wrapped != null)
                return GetContentWithKey(wrapped.Unwrap());

            return null;
        }

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {

            config.CreateMap<IPublishedContent, PublishedContentRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.CreateDate, expression => expression.MapFrom(x => x.CreateDate.ToUniversalTime())) 
                .ForMember(representation => representation.UpdateDate, expression => expression.MapFrom(x => x.UpdateDate.ToUniversalTime())) 
                .ForMember(representation => representation.Key, expression => expression.MapFrom(x => GetContentWithKey(x)))
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
