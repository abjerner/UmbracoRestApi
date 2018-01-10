namespace Umbraco.RestApi.Models
{
    public class MediaRepresentation : ContentRepresentationBase
    {
        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //required link to self
            Href = LinkTemplates.Media.Self.CreateLink(new { id = Key }).Href;
            Rel = LinkTemplates.Media.Self.Rel;

            Links.Add(LinkTemplates.Media.Root);

            Links.Add(LinkTemplates.Media.PagedChildren.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Media.PagedDescendants.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Media.Parent.CreateLink(new { parentId = ParentId }));

            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Key));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Key));

            //file upload
            Links.Add(LinkTemplates.Media.Upload.CreateLinkTemplate(Key));
        }
    }
}