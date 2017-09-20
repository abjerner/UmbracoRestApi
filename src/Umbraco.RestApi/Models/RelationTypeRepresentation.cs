using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationTypeRepresentation : Representation
    {
        protected RelationTypeRepresentation()
        {

        }

        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }

        [Display(Name = "alias")]
        public string Alias { get; set; }
 
        [Display(Name = "bidirectional")]
        public bool IsBidirectional { get; set; }

        //TODO: relations can be between more than these types
        [Display(Name = "parentEntityType")]
        public PublishedItemType ParentEntityType { get; set; }

        //TODO: relations can be between more than these types
        [Display(Name = "childEntityType")]
        public PublishedItemType ChildEntityType { get; set; }
    }
}
