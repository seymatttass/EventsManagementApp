using EventManagementApp.Data.Entities;
using EventManagementApp.Data.Context;
using EventManagementApp.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementApp.Data.Repositories
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(EventManagementContext context) : base(context) { }

        public async Task<IEnumerable<Event>> GetEventsByTypeAsync(int eventTypeId)
        {
            return await _dbSet.Where(e => e.EventTypeId == eventTypeId).ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _dbSet.Where(e => e.EventDate > DateTime.Now).ToListAsync();
        }

        public async Task<IEnumerable<Event>> SearchEventsByTitleAsync(string title)
        {
            return await _dbSet.Where(e => e.Title.Contains(title)).ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet.Where(e => e.EventDate >= startDate && e.EventDate <= endDate).ToListAsync();
        }
    }
}