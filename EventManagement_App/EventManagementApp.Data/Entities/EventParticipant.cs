using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementApp.Data.Entities
{
    public class EventParticipant : BaseEntity
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [ForeignKey("ParticipantId")]
        public virtual Participant Participant { get; set; }
    }
}