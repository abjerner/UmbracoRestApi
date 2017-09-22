using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationTypeRepresentation : Representation
    {
        public override string Rel
        {
            get { return LinkTemplates.Relations.RelationType.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.Relations.RelationType.CreateLink(new { alias = Alias }).Href; }
            set { }
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

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();
            Links.Add(LinkTemplates.Media.Root);
        }
    }
}
