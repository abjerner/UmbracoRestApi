using System;
using System.ComponentModel.DataAnnotations;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class EntityRepresentation : Representation
    {
        protected EntityRepresentation()
        {
        }

        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "parentId")]
        public int ParentId { get; set; }
        public string Path { get; set; }
        public bool HasChildren { get; set; }
        public int Level { get; set; }
        public int Id { get; set; }
        public Guid Key { get; set; }
        public int SortOrder { get; set; }

    }
}
