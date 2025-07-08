using EventManagementApp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagementApp.Business.Interfaces
{
    public interface IEventTypeService
    {
        Task<EventType> GetEventTypeByIdAsync(int id);
        Task<IEnumerable<EventType>> GetAllEventTypesAsync();
        Task<EventType> CreateEventTypeAsync(EventType eventType);
        Task<EventType> UpdateEventTypeAsync(EventType eventType);
        Task<bool> DeleteEventTypeAsync(int id);
    }
}