namespace Umbraco.RestApi.Models
{
    public class MediaRepresentation : ContentRepresentationBase
    {
        public override string Rel
        {
            get { return LinkTemplates.Media.Self.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.Media.Self.CreateLink(new { id = Id }).Href; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Links.Add(LinkTemplates.Media.Root);

            Links.Add(LinkTemplates.Media.PagedChildren.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Media.PagedDescendants.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Media.Parent.CreateLink(new { parentId = ParentId }));

            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Id));

            //file upload
            Links.Add(LinkTemplates.Media.Upload.CreateLinkTemplate(Id));
        }
    }
}