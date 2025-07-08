using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventManagementApp.Data.Entities
{
    public class EventType : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
        public virtual ICollection<Event> Events { get; set; } = new HashSet<Event>();
    }
}