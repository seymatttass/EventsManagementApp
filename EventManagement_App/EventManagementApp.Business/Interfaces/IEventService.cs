using EventManagementApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagementApp.Business.Interfaces
{
    public interface IEventService
    {
        Task<Event> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> CreateEventAsync(Event eventEntity);
        Task<Event> UpdateEventAsync(Event eventEntity);
        Task<bool> DeleteEventAsync(int id);
        Task<IEnumerable<Event>> SearchEventsByTitleAsync(string title);
        Task<IEnumerable<Event>> GetEventsByTypeAsync(int eventTypeId);

    }
}