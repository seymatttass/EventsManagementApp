using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementApp.Data.Entities
{
    public class Participant : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public virtual ICollection<EventParticipant> EventParticipants { get; set; } = new HashSet<EventParticipant>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}