using EventManagementApp.Data.Entities;
using EventManagementApp.Data.Context;
using EventManagementApp.Data.Interfaces;

namespace EventManagementApp.Data.Repositories
{
    public class EventTypeRepository : BaseRepository<EventType>, IEventTypeRepository
    {
        public EventTypeRepository(EventManagementContext context) : base(context)
        {
        }
    }
}