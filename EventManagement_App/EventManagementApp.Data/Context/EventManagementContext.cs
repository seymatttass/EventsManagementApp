using EventManagementApp.Data.Entities;
using System.Data.Entity;

namespace EventManagementApp.Data.Context
{
    public class EventManagementContext : DbContext
    {
        public EventManagementContext() : base("DefaultConnection")
        {
            Database.SetInitializer<EventManagementContext>(null);
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasRequired(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventParticipant>()
                .HasRequired(ep => ep.Event)
                .WithMany(e => e.EventParticipants)
                .HasForeignKey(ep => ep.EventId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventParticipant>()
                .HasRequired(ep => ep.Participant)
                .WithMany(p => p.EventParticipants)
                .HasForeignKey(ep => ep.ParticipantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventParticipant>()
                .HasIndex(ep => new { ep.EventId, ep.ParticipantId })
                .IsUnique();

            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}