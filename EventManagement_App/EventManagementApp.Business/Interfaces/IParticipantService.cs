using EventManagementApp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagementApp.Business.Interfaces
{
    public interface IParticipantService
    {
        Task<Participant> GetParticipantByIdAsync(int id);
        Task<IEnumerable<Participant>> GetAllParticipantsAsync();
        Task<Participant> CreateParticipantAsync(Participant participant);
        Task<bool> DeleteParticipantAsync(int id);
        Task<bool> AddParticipantToEventAsync(int eventId, int participantId);
        Task<bool> RemoveParticipantFromEventAsync(int eventId, int participantId);
        Task<IEnumerable<Event>> GetParticipantEventsAsync(int participantId);
        Task<Participant> UpdateParticipantAsync(Participant participant); 
    }
}