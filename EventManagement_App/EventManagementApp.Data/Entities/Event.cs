using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementApp.Data.Entities
{
    public class Event : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string Location { get; set; }

        [Required]
        public int EventTypeId { get; set; }

        [ForeignKey("EventTypeId")]
        public virtual EventType EventType { get; set; }

        public virtual ICollection<EventParticipant> EventParticipants { get; set; } = new HashSet<EventParticipant>();
    }
}