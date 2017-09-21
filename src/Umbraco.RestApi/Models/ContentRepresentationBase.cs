using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;
using Umbraco.RestApi.Serialization;

namespace Umbraco.RestApi.Models
{
    /// <summary>
    /// Used for Content and Media
    /// </summary>
    public abstract class ContentRepresentationBase : EntityRepresentation
    {
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }
     
        [Required]
        [Display(Name = "contentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<object>))]
        public IDictionary<string, object> Properties { get; set; }
    }

}