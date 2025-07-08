using EventManagementApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagementApp.Data.Interfaces
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        Task<IEnumerable<Event>> GetEventsByTypeAsync(int eventTypeId);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> SearchEventsByTitleAsync(string title);
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}