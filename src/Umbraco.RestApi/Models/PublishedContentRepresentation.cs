namespace Umbraco.RestApi.Models
{
    public class PublishedContentRepresentation : ContentRepresentationBase
    {
        protected PublishedContentRepresentation()
        {}
       
        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        
        public string UrlName { get; set; }
        public string Url { get; set; }
        
        public override string Rel
        {
            get { return LinkTemplates.PublishedContent.Self.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.PublishedContent.Self.CreateLink(new { id = Id }).Href; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Links.Add(LinkTemplates.PublishedContent.Root);

            Links.Add( LinkTemplates.PublishedContent.PagedChildren.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.PublishedContent.PagedDescendants.CreateLinkTemplate(Id)); ;
            Links.Add(LinkTemplates.PublishedContent.PagedAncestors.CreateLinkTemplate(Id));

            Links.Add(LinkTemplates.PublishedContent.Parent.CreateLink(new { parentId = ParentId }));
            
            //links to the relations api
            Links.Add(LinkTemplates.Relations.Children.CreateLinkTemplate(Id));
            Links.Add(LinkTemplates.Relations.Parents.CreateLinkTemplate(Id));
        }
    }

    
}
