using System;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core;
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

        /// <summary>
        /// The INT Id for the entity
        /// </summary>
        /// <remarks>
        /// This is readonly
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The Guid for the entity
        /// </summary>
        /// <remarks>
        /// This is readonly
        /// </remarks>
        public Guid Key { get; set; }

        /// <summary>
        /// The UDI for the entity
        /// </summary>
        /// <remarks>
        /// This is readonly
        /// </remarks>
        public Udi Udi { get; set; }

        public int SortOrder { get; set; }

    }
}
