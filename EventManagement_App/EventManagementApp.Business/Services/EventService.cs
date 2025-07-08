using EventManagementApp.Business.Interfaces;
using EventManagementApp.Data.Entities;
using EventManagementApp.Data.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementApp.Business.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger _logger;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
            _logger = Log.ForContext<EventService>();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            try
            {
                _logger.Information("Etkinlik getiriliyor. EventId: {EventId}", id);

                if (id <= 0) return null;

                var eventEntity = await _eventRepository.GetByIdAsync(id);

                _logger.Information(eventEntity == null
                    ? "Etkinlik bulunamadı. EventId: {EventId}"
                    : "Etkinlik getirildi. EventId: {EventId}, Title: {EventTitle}",
                    id, eventEntity?.Title);

                return eventEntity;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik getirme hatası. EventId: {EventId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            try
            {
                _logger.Information("Tüm etkinlikler getiriliyor");

                var events = await _eventRepository.GetAllAsync();

                _logger.Information("Etkinlikler getirildi. Toplam: {EventCount}",
                    events?.Count() ?? 0);

                return events ?? new List<Event>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik listesi getirme hatası");
                throw;
            }
        }

        public async Task<Event> CreateEventAsync(Event eventEntity)
        {
            try
            {
                _logger.Information("Etkinlik oluşturuluyor. Title: {EventTitle}", eventEntity?.Title);

                if (eventEntity == null)
                    throw new ArgumentNullException(nameof(eventEntity));

                if (eventEntity.EventDate <= DateTime.Now)
                    throw new ArgumentException("Etkinlik tarihi gelecekte olmalıdır.");

                var createdEvent = await _eventRepository.AddAsync(eventEntity);

                _logger.Information("Etkinlik oluşturuldu. EventId: {EventId}", createdEvent.Id);
                return createdEvent;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik oluşturma hatası. Title: {EventTitle}", eventEntity?.Title);
                throw;
            }
        }

        public async Task<Event> UpdateEventAsync(Event eventEntity)
        {
            try
            {
                _logger.Information("Etkinlik güncelleniyor. EventId: {EventId}", eventEntity?.Id);

                if (eventEntity == null)
                    throw new ArgumentNullException(nameof(eventEntity));

                if (eventEntity.EventDate <= DateTime.Now)
                    throw new ArgumentException("Etkinlik tarihi gelecekte olmalıdır.");

                var updatedEvent = await _eventRepository.UpdateAsync(eventEntity);

                _logger.Information("Etkinlik güncellendi. EventId: {EventId}", updatedEvent.Id);
                return updatedEvent;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik güncelleme hatası. EventId: {EventId}", eventEntity?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                _logger.Information("Etkinlik siliniyor. EventId: {EventId}", id);

                if (id <= 0) return false;

                var result = await _eventRepository.DeleteAsync(id);

                _logger.Information("Etkinlik silme {Result}. EventId: {EventId}",
                    result ? "başarılı" : "başarısız", id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik silme hatası. EventId: {EventId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> SearchEventsByTitleAsync(string title)
        {
            try
            {
                _logger.Information("Etkinlik aranıyor. SearchTitle: {SearchTitle}", title);

                if (string.IsNullOrWhiteSpace(title))
                    return new List<Event>();

                var events = await _eventRepository.SearchEventsByTitleAsync(title);

                _logger.Information("Arama tamamlandı. Bulunan: {EventCount}", events?.Count() ?? 0);
                return events ?? new List<Event>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Etkinlik arama hatası. SearchTitle: {SearchTitle}", title);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetEventsByTypeAsync(int eventTypeId)
        {
            try
            {
                _logger.Information("Türe göre etkinlikler getiriliyor. EventTypeId: {EventTypeId}", eventTypeId);

                if (eventTypeId <= 0)
                    return new List<Event>();

                var events = await _eventRepository.GetEventsByTypeAsync(eventTypeId);

                _logger.Information("Türe göre etkinlikler getirildi. Bulunan: {EventCount}", events?.Count() ?? 0);
                return events ?? new List<Event>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Türe göre etkinlik getirme hatası. EventTypeId: {EventTypeId}", eventTypeId);
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            try
            {
                _logger.Information("Yaklaşan etkinlikler getiriliyor");
                var events = await _eventRepository.GetUpcomingEventsAsync();
                return events ?? new List<Event>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Yaklaşan etkinlik getirme hatası");
                throw;
            }
        }
    }
}