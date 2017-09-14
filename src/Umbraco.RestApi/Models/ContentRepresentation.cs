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

        public override string Rel
        {
            get { return LinkTemplates.Content.Self.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.Content.Self.CreateLink(new { id = Id }).Href; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Links.Add(LinkTemplates.Content.Root);

            Links.Add(LinkTemplates.Content.PagedChildren.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Content.PagedDescendants.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Content.PagedAncestors.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Content.Parent.CreateLink(new { parentId = ParentId }));

            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Id));

            //file upload
            Links.Add(LinkTemplates.Media.Upload.CreateLink(new { id = Id }));
        }
    }
}
