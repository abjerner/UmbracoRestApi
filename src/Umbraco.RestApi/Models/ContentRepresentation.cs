using System.ComponentModel.DataAnnotations;

namespace Umbraco.RestApi.Models
{
    public class ContentRepresentation : ContentRepresentationBase
    {
        public ContentRepresentation()
        {
        }
        
        [Required]
        [Display(Name = "templateId")]
        public int TemplateId { get; set; }

        [Required]
        [Display(Name = "published")]
        public bool Published { get; set; }
        
        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //required link to self
            Href = LinkTemplates.Content.Self.CreateLink(new { id = Key }).Href;
            Rel = LinkTemplates.Content.Self.Rel;

            Links.Add(LinkTemplates.Content.Root);

            Links.Add(LinkTemplates.Content.PagedChildren.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Content.PagedDescendants.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Content.PagedAncestors.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Content.Parent.CreateLink(new { parentId = ParentId }));

            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Key));

            //file upload
            Links.Add(LinkTemplates.Media.Upload.CreateLink(new { id = Key }));
        }
    }
}
