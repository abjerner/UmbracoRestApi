using System.ComponentModel.DataAnnotations;

namespace Umbraco.RestApi.Models
{
    public class MemberRepresentation : ContentRepresentationBase
    {
       
        [Required]
        [Display(Name = "userName")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "email")]
        public string Email { get; set; }

        public override string Rel
        {
            get { return LinkTemplates.Members.Self.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.Members.Self.CreateLink(new { id = Id }).Href; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();            
            
            Links.Add(LinkTemplates.Members.Root);
            Links.Add(LinkTemplates.Members.MetaData.CreateLink(new { id = Id }));
        }
    }
}