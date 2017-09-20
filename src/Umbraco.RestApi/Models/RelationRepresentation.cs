using System;
using Umbraco.Core.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationRepresentation : Representation
    {
        private readonly Link _parentLink;
        private readonly Link _childLink;

        public RelationRepresentation()
        {
        }

        public RelationRepresentation(Link parentLink, Link childLink)
        {
            _parentLink = parentLink;
            _childLink = childLink;
        }

        public int Id { get; set; }

        public int ChildId { get; set; }
        public int ParentId { get; set; }

        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string RelationTypeAlias { get; set; }

        
        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //link to self
            Href = LinkTemplates.Relations.Self.CreateLink(new { id = Id }).Href;
            Rel = LinkTemplates.Relations.Self.Rel;
            

            if(_parentLink != null)
            {
                Links.Add(_parentLink);
            }

            if (_childLink != null)
            {
                Links.Add(_childLink);
            }
            
        }


    }
}
