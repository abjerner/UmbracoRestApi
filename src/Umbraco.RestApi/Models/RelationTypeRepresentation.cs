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

        [Display(Name = "parententitytype")]
        public PublishedItemType ParentEntityType { get; set; }

        [Display(Name = "childentitytype")]
        public PublishedItemType ChildEntityType { get; set; }
    }
}
